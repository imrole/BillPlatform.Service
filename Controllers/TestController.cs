using BillPlatform.IBLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillPlatform.Service.Controllers
{
    [ApiController]
    [Route("[controller]-[action]")]
    public class TestController : Controller
    {
        private ITestBLL testBLL;

        public TestController(ITestBLL testBLL)
        {
            this.testBLL = testBLL;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetCityNameByProID(int proID)
        {
            return Ok(testBLL.GetCityNameByProID(proID));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetProIDandName()
        {
            return Ok(testBLL.GetProIDandName());
        }
    }
}
