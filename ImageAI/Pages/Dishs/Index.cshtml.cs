﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ImageAI.Models;
using ImageAI.Utilities;
using System.IO;
using ImageAI.BaiduClient;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ImageAI.Pages.Dishs
{
    public class IndexModel : PageModel
    {
        private readonly ImageAI.Models.ImageAIContext _context;

        public IndexModel(ImageAI.Models.ImageAIContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "必须上传图片")]
        public IFormFile ImageUpload { get; set; }

        public IList<Dish> Dish { get; set; }

        public async Task OnGetAsync()
        {
            Dish = await _context.Dish.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Dish = await _context.Dish.AsNoTracking().ToListAsync();
                return Page();
            }

            var ImgContent = await FileHelpers.ProcessFormFile(ImageUpload, ModelState);

            if (!ModelState.IsValid)
            {
                Dish = await _context.Dish.AsNoTracking().ToListAsync();
                return Page();
            }

            var ImageClassify = new ImageClassifyClient().ImageClassify;
            var options = new Dictionary<string, object> { };
            var result = ImageClassify.DishDetect(Convert.FromBase64String(ImgContent), options);
            var title = Path.GetFileNameWithoutExtension(ImageUpload.FileName);

            var dish = new Dish
            {
                Title = title,
                ImgSize = ImageUpload.Length,
                ImgType = ImageUpload.ContentType,
                ImgContent = ImgContent,
                Data = result.ToString(),
                UploadDT = DateTime.UtcNow
            };

            _context.Dish.Add(dish);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
