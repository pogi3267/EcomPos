using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Products;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text.Json;
using Image = SixLabors.ImageSharp.Image;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly IImageProcessing _image;
        private readonly IUploadService _uploadService;
        private readonly IProductStockService _productStockService;
        private readonly IVatAndTaxService _vatAndTaxService;
        private readonly IProductTaxService _productTaxService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusinessSettingService _businessService;

        public ProductController(IProductService service,
                                IImageProcessing imageProcessing,
                                IUploadService uploadService,
                                IProductStockService productStockService,
                                IVatAndTaxService vatAndTaxService,
                                IProductTaxService productTaxService,
                                UserManager<ApplicationUser> userManager,
                                IBusinessSettingService businessService
            )
        {
            _service = service;
            _image = imageProcessing;
            _uploadService = uploadService;
            _productStockService = productStockService;
            _vatAndTaxService = vatAndTaxService;
            _productTaxService = productTaxService;
            _userManager = userManager;
            _businessService = businessService;
        }

        [HttpGet("GetInitial")]
        public async Task<IActionResult> GetInitial()
        {
            try
            {
                Product data = await _service.GetInitial();
                if (data == null) return NotFound("Data not found");
                BusinessSetting businessSetting = await _businessService.GetAsync("shipping_configuration_enabled");
                data.IsShippingEnable = Convert.ToBoolean(businessSetting.Value);

                businessSetting = await _businessService.GetAsync("stock_visibility_enabled");
                data.IsStockVisibilityEnable = Convert.ToBoolean(businessSetting.Value);

                businessSetting = await _businessService.GetAsync("flash_deal_enabled");
                data.IsFlashDealEnable = Convert.ToBoolean(businessSetting.Value);

                businessSetting = await _businessService.GetAsync("vat_tax_enabled");
                data.IsVatTaxEnable = Convert.ToBoolean(businessSetting.Value);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            #region default intialization

            var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string status = Request.Form["status"].ToString();

            #endregion default intialization

            var result = await _service.GetDataFromDbase(searchValue, pageSize, skip, sortColumn, sortColumnDir, status);
            int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            return Ok(new
            {
                draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            });
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            try
            {
                Product data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveAsync([FromForm] Product model)
        {
            try
            {
                var productValidator = new ProductValidator();
                var result = productValidator.Validate(model);
                if (!result.IsValid)
                {
                    return BadRequest(result.Errors);
                }

                Product entity;

                // Prepare product variants from the model
                var productVariants = new List<ProductVariant>();
                if (!string.IsNullOrEmpty(model.Variation))
                {
                    var variantsFromForm = JsonSerializer.Deserialize<List<ProductVariant>>(model.Variation);
                    if (variantsFromForm != null && variantsFromForm.Any())
                    {
                        foreach (var variant in variantsFromForm)
                        {
                            variant.Variant = variant.Variant?.Replace(" ", "").Trim();
                            variant.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                            productVariants.Add(variant);
                        }
                    }
                }
                if (model.ProductId > 0)
                {
                    entity = await _service.GetProductAsync(model.ProductId);
                    entity.Name = model.Name;
                    entity.CategoryId = model.CategoryId;
                    entity.BrandId = model.BrandId;
                    entity.UnitId = model.UnitId;
                    entity.MinQuantity = model.MinQuantity;
                    entity.Tags = model.Tags;
                    entity.Description = model.Description;

                    entity.Photos = model.Photos;
                    entity.ThumbnailImage = model.ThumbnailImage;

                    entity.VideoProvider = model.VideoProvider;
                    entity.VideoLink = model.VideoLink;

                    entity.Colors = model.Colors;
                    entity.ColorIds = model.ColorIds;
                    entity.AttributeIds = model.AttributeIds;
                    entity.Attributes = model.Attributes;
                    entity.AuctionProduct = model.AuctionProduct;
                    entity.Barcode = model.Barcode;
                    entity.CashOnDelivery = model.CashOnDelivery;
                    entity.InHouseProduct = model.InHouseProduct;
                    entity.ChoiceOptions = model.ChoiceOptions;
                    entity.CurrentStock = model.CurrentStock;
                    entity.Digital = model.Digital;
                    entity.Discount = model.Discount;

                    entity.DiscountEndDate = model.DiscountEndDate.HasValue ? model.DiscountEndDate.Value.ToUniversalTime() : (DateTime?)null;
                    entity.DiscountStartDate = model.DiscountStartDate.HasValue ? model.DiscountStartDate.Value.ToUniversalTime() : (DateTime?)null;
                    entity.DiscountType = model.DiscountType;
                    entity.EstShippingDays = model.EstShippingDays;
                    entity.ExternalLink = model.ExternalLink;
                    entity.ExternalLinkButton = model.ExternalLinkButton;
                    entity.Featured = model.Featured;
                    entity.FileName = model.FileName;
                    entity.FilePath = model.FilePath;
                    entity.FlashDealId = model.FlashDealId;
                    entity.FlashDiscount = model.FlashDiscount;
                    entity.FlashDiscountType = model.FlashDiscountType;
                    entity.HasVariation = model.HasVariation;
                    entity.ImageLink = model.ImageLink;
                    entity.IsQuantityMultiplied = model.IsQuantityMultiplied;
                    entity.LowStaockQuantity = model.LowStaockQuantity;
                    entity.MetaDescription = model.MetaDescription;
                    entity.MetaImage = model.MetaImage;
                    entity.MetaImageSource = model.MetaImageSource;
                    entity.MetaTittle = model.MetaTittle;
                    entity.ProductSKU = model.ProductSKU;
                    entity.SalePrice = model.SalePrice;
                    entity.WholesalePrice = model.WholesalePrice;

                    entity.SellerFeatured = model.SellerFeatured;
                    entity.ShippingCost = model.ShippingCost;
                    entity.ShippingType = model.ShippingType;
                    entity.Slug = model.Slug;
                    entity.StockVisibilityState = model.StockVisibilityState;
                    entity.Tax = model.Tax;
                    entity.TaxId = model.TaxId;
                    entity.TaxType = model.TaxType;
                    entity.ThumbnailImageSource = model.ThumbnailImageSource;
                    entity.TodaysDeal = model.TodaysDeal;
                    entity.UnitId = model.UnitId;
                    entity.UnitPrice = model.UnitPrice;
                    entity.VariantProduct = model.VariantProduct;
                    entity.Variation = model.Variation;
                    entity.WholeSaleProduct = model.WholeSaleProduct;
                    entity.Variations = model.Variation;
                    entity.EntityState = EntityState.Modified;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                    if (model.Published == 1) entity.Published = 1;

                    if (model.PdfUpload != null)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + model.PdfUpload.FileName;
                        string path = _image.GetImagePath(fileName, "PDF");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            model.PdfUpload.CopyTo(stream);
                        }
                        entity.PDF = _image.GetImagePathForDb(path);
                    }

                    #region  Product Variant

                    entity.ProductVariant.ForEach(item => item.EntityState = EntityState.Deleted);

                    // Add new or updated variants
                    foreach (var variant in productVariants)
                    {
                        var existingVariant = entity.ProductVariant.FirstOrDefault(v => v.Variant == variant.Variant);
                        if (existingVariant != null)
                        {
                            // If variant exists, update its properties
                            existingVariant.Price = variant.Price;
                            existingVariant.SKU = variant.SKU;
                            existingVariant.Image = variant.Image;
                            existingVariant.EntityState = EntityState.Modified;
                            existingVariant.Updated_At = DateTime.UtcNow;
                            existingVariant.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                        }
                        else
                        {
                            // If it's a new variant, add it
                            variant.ProductId = entity.ProductId;
                            variant.EntityState = EntityState.Added;
                            entity.ProductVariant.Add(variant);
                        }
                    }


                    #endregion Product Variant

                    // vat and tax
                    entity.ProductTaxes.ForEach(item => item.EntityState = EntityState.Deleted);
                    ProductTax tax = new ProductTax()
                    {
                        ProductId = entity.ProductId,
                        TaxType = model.TaxType,
                        Amount = Convert.ToDecimal(model.Tax),
                        Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value,
                        EntityState = EntityState.Added
                    };
                    entity.ProductTaxes.Add(tax);

                    #region Flash deals

                    entity.FlashDealProducts.ForEach(item => item.EntityState = EntityState.Deleted);
                    FlashDealProducts flashDeal = new FlashDealProducts()
                    {
                        ProductId = entity.ProductId,
                        FlashDealId = entity.FlashDealId,
                        Discount = entity.FlashDiscount,
                        DiscountType = entity.FlashDiscountType,
                        Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value,
                        EntityState = EntityState.Added
                    };
                    entity.FlashDealProducts.Add(flashDeal);

                    #endregion Flash deals

                    await _service.UpdateAsync(entity);
                }
                else
                {
                    if (model.UnitId == 0)
                    {
                        return BadRequest("Please select unit!");
                    }

                    entity = model;
                    entity.AddedBy = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                    entity.DiscountStartDate = model.DiscountStartDate.HasValue ? model.DiscountStartDate.Value.ToUniversalTime() : (DateTime?)null;
                    entity.DiscountEndDate = model.DiscountEndDate.HasValue ? model.DiscountEndDate.Value.ToUniversalTime() : (DateTime?)null;
                    if (model.Published == 1) entity.Published = 1;

                    entity.Variations = model.Variation;
                    if (entity.PdfUpload != null)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.PdfUpload.FileName;
                        string path = _image.GetImagePath(fileName, "PDF");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            model.PdfUpload.CopyTo(stream);
                        }
                        entity.PDF = _image.GetImagePathForDb(path);
                    }

                    entity.ProductVariant = productVariants;

                    // vat and tax
                    ProductTax tax = new ProductTax()
                    {
                        TaxId = model.TaxId,
                        TaxType = model.TaxType,
                        Amount = Convert.ToDecimal(model.Tax),
                        Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value,
                    };

                    FlashDealProducts flashDeal = new FlashDealProducts()
                    {
                        FlashDealId = entity.FlashDealId,
                        Discount = entity.FlashDiscount,
                        DiscountType = entity.FlashDiscountType,
                        Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value,
                    };
                    await _service.SaveAsync(entity, entity.ProductVariant, tax, flashDeal);
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                Product entity = await _service.GetProductAsync(id);
                if (entity == null) return NotFound("Data not found");
                entity.EntityState = EntityState.Deleted;
                entity.FlashDealProducts.ForEach(item => item.EntityState = EntityState.Deleted);
                entity.ProductVariant.ForEach(item => item.EntityState = EntityState.Deleted);
                entity.ProductTaxes.ForEach(item => item.EntityState = EntityState.Deleted);
                await _service.UpdateAsync(entity);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        #region Product combination

        [HttpGet("GetAttribute/{search}")]
        public async Task<IActionResult> GetAttribute(string search)
        {
            return Ok(await _service.GetAttribute(search));
        }

        #endregion Product combination

        #region Image Upload

        [HttpPost("SaveImage")]
        public async Task<IActionResult> SaveImage()
        {
            try
            {
                var photos = Request.Form.Files;
                var uploads = new List<Upload>();
                const int maxSizeInBytes = 512 * 1024; // 512 KB
                const int minSizeInBytes = 50 * 1024;  // 50 KB

                foreach (var item in photos)
                {
                    if (item.Length > maxSizeInBytes)
                    {
                        return BadRequest("One or more images exceed the maximum allowed size of 0.5MB/512 KB.");
                    }

                    // Common variables
                    string extension = Path.GetExtension(item.FileName);
                    string fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_Image" + extension;
                    string imagePath = _image.GetImagePath(fileName, "Images");
                    string dbPath = _image.GetImagePathForDb(imagePath);

                    if (item.Length < minSizeInBytes)
                    {
                        // Small image - save directly
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await item.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        // Larger image - compress
                        using (var image = await Image.LoadAsync(item.OpenReadStream()))
                        {
                            int quality = 90;
                            using (var memoryStream = new MemoryStream())
                            {
                                while (true)
                                {
                                    memoryStream.Position = 0;
                                    memoryStream.SetLength(0);

                                    var encoder = new JpegEncoder { Quality = quality };
                                    await image.SaveAsync(memoryStream, encoder);

                                    if (memoryStream.Length <= 100 * 1024 || quality <= 10)
                                        break;

                                    quality -= 5;
                                }

                                // Save to file
                                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                                {
                                    memoryStream.Position = 0;
                                    await memoryStream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }

                    var fileInfo = new FileInfo(imagePath);
                    uploads.Add(new Upload
                    {
                        EntityState = EntityState.Added,
                        FileName = dbPath,
                        FileOriginalName = item.FileName,
                        FileSize = fileInfo.Length,
                        Extension = extension,
                        Type = item.ContentType
                    });
                }

                int savedCount = await _uploadService.SaveAllAsync(uploads);
                return Ok(savedCount);
            }
            catch (Exception ex)
            {
                return BadRequest($"Image upload failed: {ex.Message}");
            }
        }


        #endregion Image Upload

        [HttpPut("UpdateProductFeature/{productId}/{featureName}/{isChecked}")]
        public async Task<IActionResult> UpdateProductFeature(int productId, string featureName, bool isChecked)
        {
            try
            {
                var product = await _service.GetProductAsync(productId);

                if (featureName == "todaysDeal")
                {
                    product.TodaysDeal = isChecked;
                }
                else if (featureName == "published")
                {
                    product.Published = isChecked == true ? 1 : 0;
                }
                else if (featureName == "featured")
                {
                    product.Featured = isChecked;
                }
                else if (featureName == "trending")
                {
                    product.IsTrend = isChecked;
                }
                await _service.UpdateProductAsync(product);
                return Ok(productId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetProductStock/{productId}")]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            try
            {
                return Ok(await _productStockService.GetProductStock(productId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateProductStock")]
        public async Task<IActionResult> UpdateProductStock(List<ProductStockHelper> productStockHelpers)
        {
            try
            {
                if (productStockHelpers.Count > 0)
                {
                    int prodId = 0, addStockQty = 0;
                    List<object> stocks = new List<object>();
                    foreach (var item in productStockHelpers)
                    {
                        addStockQty += item.Quantity;
                        var getProductStock = await _productStockService.GetSingleStock(item.ProductStockId);
                        getProductStock.Quantity += item.Quantity;
                        getProductStock.PurchaseQty += item.Quantity;
                        getProductStock.EntityState = EntityState.Modified;
                        await _productStockService.SaveSingleAsync(getProductStock);
                        prodId = getProductStock.ProductId;
                        object product = new
                        {
                            Variant = getProductStock.Variant,
                            Price = getProductStock.Price,
                            SKU = getProductStock.SKU,
                            Quantity = getProductStock.Quantity,
                            Image = getProductStock.Image
                        };

                        stocks.Add(product);
                    }

                    if (prodId > 0)
                    {
                        string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(stocks);
                        var product = await _service.GetProductAsync(prodId);
                        product.Variations = jsonResult;
                        product.CurrentStock += addStockQty;
                        await _service.UpdateProductAsync(product);
                    }



                }

                return Ok("Successfully Product Stock Updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("EditProduct/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                Product data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");

                List<object> variants = new List<object>();
                foreach (ProductVariant item in data.ProductVariant)
                {
                    object stock = new
                    {
                        Variant = item.Variant,
                        Price = item.Price,
                        SKU = item.SKU,
                        ImageUrl = item.ImageUrl,
                        Image = item.Image
                    };
                    variants.Add(stock);
                }
                string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(variants);
                data.Variations = jsonResult;

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetFlashDealProduct/{flashDealId}")]
        public async Task<IActionResult> GetFlashDealProductAsync(int flashDealId)
        {
            Product product = new Product();
            product.FlashDealProductList = await _service.GetFlashDealProductsAsync(flashDealId);
            product.ProductList = await _service.GetProductsAsync();
            return Ok(product);
        }

        [HttpDelete("DeleteFlashDealProduct/{flashDealId}/{productId}")]
        public async Task<IActionResult> DeleteFlashDealProductAsync(int flashDealId, int productId)
        {
            try
            {
                await _service.DeleteFlashDealProductAsync(flashDealId, productId);
                return Ok(flashDealId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("SaveFlashDealProduct/{flashDealId}/{prodId}/{flashDealDiscount}/{flashDealDiscountType}")]
        public async Task<IActionResult> SaveFlashDealProductAsync(int flashDealId, int prodId, decimal? flashDealDiscount, string flashDealDiscountType)
        {
            try
            {
                List<Product> prods = await _service.GetFlashDealProductAsync(flashDealId, prodId);
                if (prods.Count > 0)
                {
                    throw new Exception("This product already exist!");
                }
                FlashDealProducts flashDealProduct = new FlashDealProducts()
                {
                    FlashDealId = flashDealId,
                    ProductId = prodId,
                    Discount = flashDealDiscount,
                    DiscountType = flashDealDiscountType,
                    Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value,
                    Created_At = DateTime.UtcNow,
                    EntityState = EntityState.Added
                };
                await _service.SaveFlashDealProductAsync(flashDealProduct);
                return Ok(flashDealProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteUploadImg/{ids}")]
        public async Task<IActionResult> DeleteUploadImg(string ids)
        {
            try
            {
                await _service.DeleteUploadImg(ids);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("GetVariant/{id}")]
        public async Task<IActionResult> GetVariantByProductAsync(int id)
        {
            List<ProductStock> stockList = await _service.GetVariantByProductAsync(id);
            return Ok(stockList);
        }

        //private async Task UpdateProductVariants(Product model, Product entity)
        //{
        //    if (string.IsNullOrEmpty(model.Variation))
        //    {
        //        // No variants, so delete existing ones.
        //        entity.Variants.ForEach(v => v.EntityState = EntityState.Deleted);
        //        return;
        //    }

        //    var newVariantsFromForm = JsonSerializer.Deserialize<List<Variant>>(model.Variation);
        //    if (newVariantsFromForm == null || !newVariantsFromForm.Any())
        //    {
        //        // Invalid or empty variant data, delete existing ones.
        //        entity.Variants.ForEach(v => v.EntityState = EntityState.Deleted);
        //        return;
        //    }

        //    var existingVariantIds = entity.Variants.Select(v => v.Variant_id).ToList();
        //    var newVariantIds = newVariantsFromForm.Select(v => v.Variant_id).ToList();

        //    // Mark variants for deletion that are no longer in the new list.
        //    var variantsToDelete = entity.Variants
        //        .Where(v => !newVariantIds.Contains(v.Variant_id))
        //        .ToList();
        //    variantsToDelete.ForEach(v => v.EntityState = EntityState.Deleted);

        //    foreach (var newVariant in newVariantsFromForm)
        //    {
        //        var existingVariant = entity.Variants.FirstOrDefault(v => v.Variant_id == newVariant.Variant_id);

        //        if (existingVariant != null)
        //        {
        //            // Update existing variant
        //            existingVariant.Sku = newVariant.Sku;
        //            existingVariant.Barcode = newVariant.Barcode;
        //            existingVariant.Attributes = newVariant.Attributes;
        //            existingVariant.Unit_id = newVariant.Unit_id;
        //            existingVariant.Regular_price = newVariant.Regular_price;
        //            existingVariant.Sale_price = newVariant.Sale_price;
        //            existingVariant.Reorder_level = newVariant.Reorder_level;
        //            existingVariant.Image = newVariant.Image;
        //            existingVariant.Status = newVariant.Status;
        //            existingVariant.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
        //            existingVariant.EntityState = EntityState.Modified;
        //        }
        //        else
        //        {
        //            // Add new variant
        //            newVariant.Product_id = entity.ProductId;
        //            newVariant.Created_By = DateTime.UtcNow;
        //            newVariant.Updated_By = DateTime.UtcNow;
        //            newVariant.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
        //            newVariant.EntityState = EntityState.Added;
        //            entity.Variants.Add(newVariant);
        //        }
        //    }
        //}

        private List<ProductVariant> CreateProductVariants(Product model)
        {
            var productVariants = new List<ProductVariant>();
            if (!string.IsNullOrEmpty(model.Variation))
            {
                var variantsFromForm = JsonSerializer.Deserialize<List<ProductVariant>>(model.Variation);
                if (variantsFromForm != null && variantsFromForm.Any())
                {
                    variantsFromForm.ForEach(x =>
                    {
                        x.Created_At = DateTime.UtcNow; // Correctly setting DateTime for Created_At
                        x.Updated_At = DateTime.UtcNow; // Correctly setting DateTime for Updated_At
                        x.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value; // Correctly setting string for Created_By
                    });
                    productVariants.AddRange(variantsFromForm);
                }
            }
            return productVariants;
        }
    }
}