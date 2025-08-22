using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/Attribute")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class AttributeController : ControllerBase
    {
        private readonly IAttributeService _service;

        public AttributeController(IAttributeService service)
        {
            _service = service;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            var result = await _service.GetDataFromDbase(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6);
            int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            return Ok(new
            {
                paginationResult.Item1,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            });
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Policy = "AttributeEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                ProductAttribute data = await _service.GetAsync(id);
                data.AttributeValueList = await _service.GetAttributeValues(id);
                data.Values = string.Join(",", data.AttributeValueList.Select(x => x.Value));

                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "AttributeCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] ProductAttribute model)
        {
            try
            {
                ProductAttribute entity = model;
                if (entity.AttributeId > 0)
                {
                    entity.EntityState = EntityState.Modified;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;

                    List<AttributeValue> attributeValues = new List<AttributeValue>();
                    entity.Values.Split(',').ToList().ForEach(val =>
                    {
                        AttributeValue attributeValue = new AttributeValue
                        {
                            Value = val
                        };
                        attributeValues.Add(attributeValue);
                    });

                    entity.AttributeValueList = await _service.GetAttributeValues(entity.AttributeId);
                    entity.AttributeValueList.SetUnchanged();

                    foreach (AttributeValue attributeValue in attributeValues)
                    {
                        AttributeValue? attribute = entity.AttributeValueList.FirstOrDefault(x => x.Value == attributeValue.Value);
                        if (attribute == null)
                        {
                            attribute = attributeValue;
                            attribute.AttributeId = entity.AttributeId;
                            attribute.EntityState = EntityState.Added;
                            entity.AttributeValueList.Add(attributeValue);
                        }
                        else
                        {
                            attribute.EntityState = EntityState.Modified;
                        }
                    }

                    entity.AttributeValueList.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
                }
                else
                {
                    entity.Created_At = DateTime.UtcNow;
                    entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.EntityState = EntityState.Added;

                    entity.Values.Split(',').ToList().ForEach(val =>
                    {
                        AttributeValue attributeValue = new AttributeValue
                        {
                            Value = val,
                            EntityState = EntityState.Added
                        };
                        entity.AttributeValueList.Add(attributeValue);
                    });
                }
                await _service.SaveAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = "AttributeDeletePolicy")]
        public async Task<IActionResult> DeleteColorAsync([FromRoute] int id)
        {
            try
            {
                ProductAttribute entity = await _service.GetAsync(id);
                entity.EntityState = EntityState.Deleted;

                entity.AttributeValueList = await _service.GetAttributeValues(entity.AttributeId);
                entity.AttributeValueList.SetDeleted();

                await _service.SaveAsync(entity);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}