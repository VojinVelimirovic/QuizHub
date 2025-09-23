using QuizHub.Services.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizHub.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryServiceDto> CreateCategoryAsync(CategoryCreateServiceDto dto);
        Task<List<CategoryServiceDto>> GetAllCategoriesAsync();
    }
}
