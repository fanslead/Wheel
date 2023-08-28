﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wheel.Core.Http;
using Wheel.Services.LocalizationManage;

namespace Wheel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizationManageController : ControllerBase
    {
        private readonly ILocalizationManageAppService _localizationManageAppService;

        public LocalizationManageController(ILocalizationManageAppService localizationManageAppService)
        {
            _localizationManageAppService = localizationManageAppService;
        }

        [HttpGet]
        public async Task<R> Get(int id)
        {
            var value = await _localizationManageAppService.GetLocalizationCulture(id);
            return new R();
        }
    }
}