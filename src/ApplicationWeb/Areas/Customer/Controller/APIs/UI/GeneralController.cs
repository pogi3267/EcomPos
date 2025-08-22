using ApplicationCore.DTOs;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Models;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.Public;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text.Json;

namespace ApplicationWeb.Areas.Customer.Controller.APIs.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly IGeneralService _service;
        private readonly IBusinessSettingService _businessService;

        private List<Category> categories = new List<Category>();

        public GeneralController(IGeneralService service,
            IProductService productService,
            IBusinessSettingService businessService)
        {
            _service = service;
            _businessService = businessService;
        }

        [HttpGet("system-setting")]
        public async Task<IActionResult> GetSystemSettingAsync()
        {
            try
            {
                dynamic setting = await _service.GetSystemSettingAsync();
                setting.HeaderImages = setting.HeaderImages.ToString().Split(",");

                setting.HomePageConfig = JsonConvert.DeserializeObject<List<SectionData>>(setting.HomePageConfig.ToString());

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("order-setting")]
        public async Task<IActionResult> GetOrderSettingAsync()
        {
            try
            {
                dynamic setting = await _service.GetOrderSettingAsync();
                setting.AddressConfig = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(setting.AddressConfig.ToString());

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("terms-privacy")]
        public async Task<IActionResult> GetTermsAndPrivacyAsync()
        {
            try
            {
                var setting = await _service.GetTermsAndPrivacyAsync();

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("category")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            try
            {
                List<Category> categories = await _service.GetCategoriesAsync();

                var data = FlatToHierarchy(categories);

                object ProjectCategory(Category category) =>
                new
                {
                    category.CategoryId,
                    category.ParentId,
                    category.ParentName,
                    category.Name,
                    category.Banner,
                    Categories = category.Categories.Select(ProjectCategory).ToList()
                };

                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    { "Categories", data.Select(ProjectCategory).ToList() }
                };

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("category-list")]
        public async Task<IActionResult> GetCategoryListAsync()
        {
            try
            {
                List<Category> data = await _service.GetCategoryListAsync();

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Explore Categories");
                dict.Add("SubTitle", "");
                dict.Add("Categories", data.Select(x => new { x.CategoryId, x.ParentId, x.Name, x.Banner }));

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public List<Category> FlatToHierarchy(List<Category> list)
        {
            var lookup = new Dictionary<int?, Category>();
            var nested = new List<Category>();
            foreach (Category item in list)
            {
                if (lookup.ContainsKey(item.ParentId))
                {
                    lookup[item.ParentId].Categories.Add(item);
                }
                else
                {
                    nested.Add(item);
                }
                lookup.Add(item.CategoryId, item);
            }

            return nested;
        }

        [HttpGet("flash-deal")]
        public async Task<IActionResult> GetFlashDealProductsAsync(int id, int noOfRows = 6)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetFlashDealProductsAsync(id, noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Flash Deal Sale");
                dict.Add("SubTitle", products.FirstOrDefault()?.FlashDealTitle);
                dict.Add("StartDate", products.FirstOrDefault()?.FlashDealStartDate);
                dict.Add("EndDate", products.FirstOrDefault()?.FlashDealEndDate);
                dict.Add("FlashDateDescription", "This deal ending in");
                dict.Add("FlashDealId", products.FirstOrDefault()?.FlashDealId);
                dict.Add("TextColor", products.FirstOrDefault()?.FlashDealTextColor);
                dict.Add("BackgroundColor", products.FirstOrDefault()?.FlashDealBackgroundColor);
                dict.Add("Banner", products.FirstOrDefault()?.FlashDealBanner);
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("todays-deal")]
        public async Task<IActionResult> GetTodaysDealProductsAsync(int noOfRows = 8)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetTodaysDealProductsAsync(noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Todays Deal Products");
                dict.Add("SubTitle", "Pick your item from todays deal products");
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("trending-product")]
        public async Task<IActionResult> GetTrendingProductsAsync(int noOfRows = 8)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetTrendingProductsAsync(noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Trending Products");
                dict.Add("SubTitle", "Pick your item from trending products");
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private List<Dictionary<string, object>> CastToDictionary(List<dynamic> products)
        {
            List<Dictionary<string, object>> prods = new List<Dictionary<string, object>>();

            products.ForEach(x =>
            {
                var dictionary = x as IDictionary<string, object>;

                if (dictionary != null)
                {
                    prods.Add(new Dictionary<string, object>(dictionary));
                }
                else
                {
                    var tempDictionary = new Dictionary<string, object>();
                    foreach (var property in x.GetType().GetProperties())
                    {
                        tempDictionary[property.Name] = property.GetValue(x);
                    }
                    prods.Add(tempDictionary);
                }
            });

            var keysToRemove = new HashSet<string> { "Photos", "ThumbnailImage", "VideoProvider", "VideoLink", "Description", "PurchasePrice", "VariantProduct", "ChoiceOptions"
            , "Colors", "Variations", "TodaysDeal", "Published", "Approved", "CashOnDelivery", "Featured", "SellerFeatured", "Discount"
            , "DiscountType", "DiscountStartDate", "DiscountEndDate", "Tax", "TaxType", "ShippingType", "ShippingCost", "IsQuantityMultiplied", "EstShippingDays", "MetaTittle"
            , "MetaDescription", "MetaImage", "PDF", "Slug", "Rating", "Barcode", "Digital", "AuctionProduct", "FileName", "FilePath", "ExternalLink", "ExternalLinkButton", "WholeSaleProduct"
            , "ProductSKU", "IsTrend", "ColorNameWithCode", "ProductAttributes"};
            var filteredProds = prods.Select(dict =>
                                dict
                                    .Where(kvp => !keysToRemove.Contains(kvp.Key)) // Exclude specified keys
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) // Re-create dictionary without those keys
                                ).ToList();

            return filteredProds;
        }

        [HttpGet("discount-products")]
        public async Task<IActionResult> GetDiscountProductsAsync(int noOfRows = 8)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetDiscountProductsAsync(noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Discount Product");
                dict.Add("SubTitle", "");
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("featured-products")]
        public async Task<IActionResult> GetFeaturedProductsAsync(int noOfRows = 8)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetFeaturedProductsAsync(noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Featured Product");
                dict.Add("SubTitle", "");
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("category-wise-products")]
        public async Task<IActionResult> GetCategoryWiseProductsAsync(int id, int noOfRows = 3)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetCategoryWiseProductsAsync(id, noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", products.FirstOrDefault()?.ParentCategoryName); //"Men Fashion"
                dict.Add("SubTitle", "");
                dict.Add("CategoryId", products.FirstOrDefault()?.ParentCategoryId);
                dict.Add("CategoryName", products.FirstOrDefault()?.ParentCategoryName);
                dict.Add("CategoryImage", products.FirstOrDefault()?.ParentCategoryBanner);
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-category-products-s1")]
        public async Task<IActionResult> GetTopFirstCategoryProductsAsync()
        {
            try
            {
                List<dynamic> products = await _service.GetTopCategoryProductsAsync(true);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", products.FirstOrDefault()?.ParentCategoryName); //"Men Fashion"
                dict.Add("SubTitle", "");
                dict.Add("CategoryId", products.FirstOrDefault()?.ParentCategoryId);
                dict.Add("CategoryName", products.FirstOrDefault()?.ParentCategoryName);
                dict.Add("CategoryImage", products.FirstOrDefault()?.ParentCategoryBanner);
                dict.Add("Products", products);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-category-products-s2")]
        public async Task<IActionResult> GetTopSecondCategoryProductsAsync()
        {
            try
            {
                List<dynamic> products = await _service.GetTopCategoryProductsAsync(false);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", products.FirstOrDefault()?.ParentCategoryName); //"Women Fashion"
                dict.Add("SubTitle", "");
                dict.Add("CategoryId", products.FirstOrDefault()?.ParentCategoryId);
                dict.Add("CategoryName", products.FirstOrDefault()?.ParentCategoryName);
                dict.Add("CategoryImage", products.FirstOrDefault()?.ParentCategoryBanner);
                dict.Add("Products", products);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("customer-reviews")]
        public async Task<IActionResult> GetCustomerReviewsAsync()
        {
            try
            {
                List<dynamic> reviews = await _service.GetCustomerReviewsAsync();
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "What customer say about us!");
                dict.Add("SubTitle", "Hear From Our Satisfied Customers");
                dict.Add("Reviews", reviews);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("special-offer-email")]
        public async Task<IActionResult> SaveSpecialOfferEmailAsync(string email)
        {
            try
            {
                SpecialOfferEmails speialOfferEmails = new SpecialOfferEmails()
                {
                    Email = email,
                    EntityState = ApplicationCore.Enums.EntityState.Added
                };
                await _service.SaveSpecialOfferEmailAsync(speialOfferEmails);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("related-products")]
        public async Task<IActionResult> GetRelatedProductsAsync(int id, int noOfRows = 8)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetRelatedProductsAsync(id, noOfRows, userId);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Title", "Related Products");
                dict.Add("SubTitle", "");
                dict.Add("Products", products);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("product-details")]
        public async Task<IActionResult> GetProductDetailsAsync(int id, string? name)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                dynamic product = await _service.GetProductDetailsAsync(id, userId);

                product.Tags = product.Tags.ToString().Split(",");
                product.ProductPhotos = product.ProductPhotos.ToString().Split(",");

                List<string> productAttributes = new List<string>();
                Dictionary<string, object> attributes = new Dictionary<string, object>();
                List<dynamic> dynObjectList = new List<dynamic>();
                dynamic dynObject = new ExpandoObject();

                string[] colors = new string[] { };
                if (!string.IsNullOrEmpty(product.ColorNameWithCode))
                {
                    colors = product.ColorNameWithCode.ToString().Split(",");
                }

                if (colors.Length > 0)
                {
                    foreach (string item in colors)
                    {
                        dynObject = new ExpandoObject();
                        string[] cols = item.Split('-');
                        dynObject.Name = cols[0];
                        dynObject.Value = cols[1];
                        dynObjectList.Add(dynObject);
                    }
                    attributes.Add("Color", dynObjectList);
                    productAttributes.Add("Color");
                }

                List<dynamic> attributeList = product.AttributeList;

                List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                if (!string.IsNullOrEmpty(product.ChoiceOptions))
                    opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(product.ChoiceOptions);

                if (opt.Count > 0)
                {
                    foreach (var item in opt)
                    {
                        dynObjectList = new List<dynamic>();
                        List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                        if (vals?.Count > 0)
                        {
                            foreach (var val in vals)
                            {
                                dynObject = new ExpandoObject();
                                dynObject.Name = val;
                                dynObject.Value = val;
                                dynObjectList.Add(dynObject);
                            }
                        }
                        var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                        if (attr != null)
                        {
                            attributes.Add(attr.Name, dynObjectList);
                            productAttributes.Add(attr.Name);
                        }
                    }
                }

                product.Attributes = attributes;
                product.ProductAttributes = productAttributes;

                List<dynamic> reviews = product.Reviews;
                reviews.ForEach(x =>
                {
                    x.Images = x.ReviewPhotos.ToString().Split(",");
                    x.IsPositive = Convert.ToDouble(x.Rating) > 3 ? "1" : "0";
                });

                Dictionary<string, object> seller = new Dictionary<string, object>();
                seller["SellerId"] = product.SellerId;
                seller["Name"] = product.SellerName;
                seller["SellerType"] = product.SellerType;
                string[] tags = new string[] { };
                if (product.SellerTags != null) tags = product.SellerTags.ToString().Split(",");
                seller["Tags"] = tags;
                seller["SellerImage"] = product.SellerImage;
                seller["IsVerified"] = product.IsVerified;
                product.SellerDetails = seller;

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Product", product);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("meta-data")]
        public async Task<IActionResult> GetMetaDataAsync()
        {
            try
            {
                List<Category> categories = await _service.GetCategoriesAsync();
                var catData = FlatToHierarchy(categories);

                var brands = await _service.GetBrandsAsync();

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Categories", catData.Select(x => new { x.CategoryId, x.ParentId, x.ParentName, x.Name, x.Banner, x.Categories }));
                dict.Add("Brands", brands);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("filter-products")]
        public async Task<IActionResult> GetFilterProductsAsync(string? sortBy, int pageNumber, int numberOfRows, string? price, string? rating, string? brand,
            string? category, bool isFlashDeal, bool isTrending, bool isDiscount, bool isFeatured, bool inStock, bool outOfStock, bool isTodaysDeal, string? keySearch)
        {
            try
            {
                ProductSearchDTO prod = new ProductSearchDTO
                {
                    SortBy = sortBy,
                    PageNumber = pageNumber,
                    NumberOfRows = numberOfRows,
                    Price = price,
                    Rating = rating,
                    Brand = brand,
                    Category = category,
                    IsFlashDeal = isFlashDeal,
                    IsTrending = isTrending,
                    IsDiscount = isDiscount,
                    IsFeatured = isFeatured,
                    InStock = inStock,
                    OutOfStock = outOfStock,
                    IsTodaysDeal = isTodaysDeal,
                    KeySearch = keySearch
                };

                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                List<dynamic> products = await _service.GetFilterProductsAsync(prod, userId);

                SearchActivity searchActivity = new SearchActivity
                {
                    Date = DateTime.UtcNow,
                    URL = HttpContext.Request.Path,
                    SearchCriteria = JsonConvert.SerializeObject(prod)
                };

                await _service.CreateUserActivities(searchActivity);

                string productIds = String.Join(",", products.Select(x => x.ProductId));
                var productStockList = await _service.GetProductStockListByProductIdsAsync(productIds);

                List<dynamic> attributeList = await _service.GetAttributesAsync();

                products.ForEach(x =>
                {
                    x.Tags = x.Tags.ToString().Split(",");
                    x.ProductStocks = productStockList.Where(p => p.ProductId == x.ProductId).ToList();
                    #region Attribute
                    List<string> productAttributes = new List<string>();
                    Dictionary<string, object> attributes = new Dictionary<string, object>();
                    List<dynamic> dynObjectList = new List<dynamic>();
                    dynamic dynObject = new ExpandoObject();

                    string[] colors = new string[] { };
                    if (!string.IsNullOrEmpty(x.ColorNameWithCode))
                    {
                        colors = x.ColorNameWithCode.ToString().Split(",");
                    }

                    if (colors.Length > 0)
                    {
                        foreach (string item in colors)
                        {
                            dynObject = new ExpandoObject();
                            string[] cols = item.Split('-');
                            dynObject.Name = cols[0];
                            dynObject.Value = cols[1];
                            dynObjectList.Add(dynObject);
                        }
                        attributes.Add("Color", dynObjectList);
                        productAttributes.Add("Color");
                    }

                    List<IDictionary<string, object>> opt = new List<IDictionary<string, object>>();
                    if (!string.IsNullOrEmpty(x.ChoiceOptions))
                        opt = JsonConvert.DeserializeObject<List<IDictionary<string, object>>>(x.ChoiceOptions);

                    if (opt.Count > 0)
                    {
                        foreach (var item in opt)
                        {
                            dynObjectList = new List<dynamic>();
                            List<string> vals = JsonConvert.DeserializeObject<List<string>>(item["value"].ToString());
                            if (vals?.Count > 0)
                            {
                                foreach (var val in vals)
                                {
                                    dynObject = new ExpandoObject();
                                    dynObject.Name = val;
                                    dynObject.Value = val;
                                    dynObjectList.Add(dynObject);
                                }
                            }
                            var attr = attributeList.FirstOrDefault(x => x.AttributeId.ToString() == item["attributeId"].ToString());
                            if (attr != null)
                            {
                                attributes.Add(attr.Name, dynObjectList);
                                productAttributes.Add(attr.Name);
                            }
                        }
                    }

                    x.Attributes = attributes;
                    x.ProductAttributes = productAttributes;
                    #endregion
                });

                List<Dictionary<string, object>> prods = CastToDictionary(products);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("MinPrice", products.Count() > 0 ? products.FirstOrDefault()?.MinPrice : 0);
                dict.Add("MaxPrice", products.Count() > 0 ? products.FirstOrDefault()?.MaxPrice : 0);
                dict.Add("Count", products.Count() > 0 ? products.FirstOrDefault()?.TotalRows : 0);
                dict.Add("Products", prods);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("footer-info")]
        public async Task<IActionResult> GetFooterInfoAsync()
        {
            try
            {
                dynamic setting = await _service.GetFooterInfoAsync();
                setting.PaymentIcons = setting.PaymentIcons.ToString().Split(",");

                return Ok(setting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("address-point")]
        public async Task<IActionResult> GetAdressPointsAsync()
        {
            try
            {
                List<dynamic> pickupList = await _service.GetPickupPointListAsync();
                List<dynamic> countryList = await _service.GetCountryListAsync();
                List<dynamic> stateList = await _service.GetStateListAsync();
                List<dynamic> cityList = await _service.GetCityListAsync();

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("PickupPoints", pickupList);
                dict.Add("Countries", countryList);
                dict.Add("States", stateList);
                dict.Add("Cities", cityList);

                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("setting-value")]
        public async Task<IActionResult> GetSettingValueAsync(string type, string key)
        {
            try
            {
                BusinessSetting businessSetting = await _businessService.GetAsync(type);

                using JsonDocument document = JsonDocument.Parse(businessSetting.Value);
                JsonElement root = document.RootElement;

                string? stripeKey = root.GetProperty(key).GetString();
                return Ok(stripeKey);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("search-result")]
        public async Task<IActionResult> GetSearchResustAsync(string keySearch)
        {
            try
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                List<string> results = await _service.GetSearchResustAsync(keySearch);
                dict.Add("Result", results);
                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("product-seo")]
        public async Task<IActionResult> GetProductDetailsForSEOAsync(int id, string? name)
        {
            try
            {
                dynamic product = await _service.GetProductDetailsForSEOAsync(id);

                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("Product", product);
                return Ok(dict);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("action-review")]
        [Authorize]
        public async Task<IActionResult> ActionToReviewAsync(int id, int action)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _service.ActionToReviewAsync(id, action, userId);

                return Ok("Success!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}