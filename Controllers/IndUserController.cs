using BillPlatform.IBLL;
using BillPlatform.Model.Request;
using BillPlatform.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillPlatform.Service.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class IndUserController : Controller
    {
        private readonly IIndUserBLL userBLL;

        public IndUserController(IIndUserBLL userBLL)
        {
            this.userBLL = userBLL;
        }

        /// <summary>
        /// 个人用户注册接口
        /// </summary>
        /// <param name="indUser"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RegisterUser(IndUserRegisterRequestDTO indUser)
        {
            //数据筛查
            if (string.IsNullOrEmpty(indUser.Email) || string.IsNullOrEmpty(indUser.UserName) || string.IsNullOrEmpty(indUser.PassWord))
            {
                return Json(new { Code = 400, Msg = "数据为空" });
            }
            if (userBLL.HaveEmail(indUser.Email))
            {
                return Json(new { Code = 400, Msg = "已经注册过，请登录或找回密码" });
            }
            //注册
            if (userBLL.Register(indUser.PassWord, indUser.UserName, indUser.Email))
            {
                return Json(new { Code = 200, Msg = "成功" });
            }
            return Json(new { Code = 400, Msg = "失败" });
        }

        /// <summary>
        /// 通过邮箱获取用户ID
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetUserIDByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { Code = 400, Msg = "数据为空" });
            }
            if (userBLL.HaveEmail(email))
            {
                return Json(new { Code = 200, Msg = "成功", Data = userBLL.GetUserIDByEmail(email) });
            }
            return Json(new { Code = 400, Msg = "邮箱不存在" });
        }

        /// <summary>
        /// 添加个人用户账单
        /// </summary>
        /// <param name="billDTO">添加账单DTO</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "IndUser")]
        public IActionResult AddIndUserBill(AddBillDTO billDTO)
        {
            if (string.IsNullOrEmpty(billDTO.BillTypeID) || string.IsNullOrEmpty(billDTO.IndUserID))
            {
                return Json(new { Code = 400, Msg = "数据为空" });
            }
            if (userBLL.AddBill(billDTO))
            {
                return Json(new { Code = 200, Msg = "成功" });
            }
            return Json(new { Code = 400, Msg = "失败" });
        }

        /// <summary>
        /// 添加账单类别
        /// </summary>
        /// <param name="billTypeDTO">账单类别DTO</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "IndUser")]
        public IActionResult AddBillType(AddBillTypeDTO billTypeDTO)
        {
            if (string.IsNullOrEmpty(billTypeDTO.UserID) || string.IsNullOrEmpty(billTypeDTO.BillTypeName) || string.IsNullOrEmpty(billTypeDTO.Icon))
            {
                return Json(new { Code = 400, Msg = "数据为空" });
            }
            if (userBLL.AddBillType(billTypeDTO))
            {
                return Json(new { Code = 200, Msg = "成功" });
            }
            return Json(new { Code = 400, Msg = "失败" });
        }

        /// <summary>
        /// 获取全部账单
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetALLBill(string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            List<IndBillDTO> indBills = userBLL.GetAllBill(userID);
            if (indBills.Count > 0)
            {
                return Json(new { Code = 200, Msg = "成功", Data = indBills });
            }
            return Json(new { Code = 400, Msg = "没有找到账单信息" });
        }

        /// <summary>
        /// 根据账单种类获取全部账单
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="typeID">种类ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetAllBillByType(string userID, string typeID)
        {
            if (string.IsNullOrEmpty(userID)||string.IsNullOrEmpty(typeID))
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            if (!userBLL.HavaUserID(userID))
            {
                return Json(new { Code = 400, Msg = "用户不存在" });
            }
            if (!userBLL.HaveTypeID(typeID))
            {
                return Json(new { Code = 400, Msg = "类别不存在" });
            }
            List<IndBillDTO> indBills = userBLL.GetAllBillByType(userID,typeID);
            if (indBills.Count > 0)
            {
                return Json(new { Code = 200, Msg = "成功", Data = indBills });
            }
            return Json(new { Code = 400, Msg = "没有找到账单信息" });
        }

        /// <summary>
        /// 获取全部账单种类
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetALLBillType(string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            List<BillTypeDTO> billTypes = userBLL.GetAllBillType(userID);
            if (billTypes.Count > 0)
            {
                return Json(new { Code = 200, Msg = "成功", Data = billTypes });
            }
            return Json(new { Code = 400, Msg = "没有找到账单类别信息" });
        }

        /// <summary>
        /// 获取用户月额度
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetMonthLimit(string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            if (userBLL.HavaUserID(userID))
            {
                return Json(new { Code = 200, Msg = "获取成功", Data = userBLL.GetMonthLimt(userID) });
            }
            return Json(new { Code = 500, Msg = "用户不存在" });
        }

        /// <summary>
        /// 修改用户每月额度
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="newLimit">新的额度</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "IndUser")]
        public IActionResult UpdateMonthLimit(UpdateMonthLimitDTO limitDTO)
        {
            if (string.IsNullOrEmpty(limitDTO.UserID) || limitDTO.NewLimit <= 0)
            {
                return Json(new { Code = 400, Msg = "参数为空或格式不正确" });
            }
            return Json(userBLL.UpdateMonthLimit(limitDTO.UserID, limitDTO.NewLimit));
        }

        /// <summary>
        /// 获取省份ID
        /// </summary>
        /// <param name="proName">省份名称</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetPromaryID(string proName)
        {
            if (string.IsNullOrEmpty(proName))
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            int ret = userBLL.GetPromaryID(proName);
            if (ret == -1)
            {
                return Json(new { Code = 400, Msg = "省份不存在，请重新输入" });
            }
            return Json(new { Code = 200, Msg = "省份ID获取成功", Data = ret });
        }

        /// <summary>
        /// 获取城市ID
        /// </summary>
        /// <param name="cityName">城市名</param>
        /// <param name="proID">省份ID</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "IndUser")]
        public IActionResult GetCityID(string cityName, int proID)
        {
            if (string.IsNullOrEmpty(cityName)||proID<=0)
            {
                return Json(new { Code = 400, Msg = "参数为空" });
            }
            int ret = userBLL.GetCityID(cityName, proID);
            if (ret == -1)
            {
                return Json(new { Code = 400, Msg = "城市不存在，请重新输入" });
            }
            return Json(new { Code = 200, Msg = "城市ID获取成功", Data = ret });
        }
    }
}
