using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using HRWebApp.Models;
using Newtonsoft.Json;

namespace HRWebApp.ApiController
{
    public class PersonalController : System.Web.Http.ApiController
    {
        private HRDB db = new HRDB(); 
        public IEnumerable<Personal> Get()
        {
            return db.Personals.ToList();
        }
        //! Zip bỏ dưới 6 số là ok
        public IHttpActionResult Post([FromBody] Personal personal)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                db.Personals.Add(personal);
                db.SaveChanges();
                var responseData = new
                {
                    Empoyee_ID = personal.Employee_ID,
                    First_Name = personal.First_Name,
                    Last_Name = personal.Last_Name,
                    Middle_Initial = personal.Middle_Initial,
                    Address1 = personal.Address1,
                    Address2 = personal.Address2,
                    City = personal.City,
                    State = personal.State,
                    Zip = personal.Zip,
                    Email = personal.Email,
                    Phone_Number = personal.Phone_Number,
                    Social_Security_Number = personal.Social_Security_Number,
                    Drivers_License = personal.Drivers_License,
                    Marital_Status = personal.Marital_Status,
                    Gender = personal.Gender,
                    Shareholder_Status = personal.Shareholder_Status,
                    Benefit_Plans = personal.Benefit_Plans,
                    Ethnicity = personal.Ethnicity
                };
                return Ok(new { success = true, data = responseData });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message;
                return Ok(new { success = false, data = errorMessage });
            }
        }

        // Update Delete cần viết thêm xóa những bảng liên quan là ok
        public IHttpActionResult Put(decimal id, [FromBody] Personal personal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != personal.Employee_ID)
            {
                return BadRequest("ID không khớp với dữ liệu.");
            }

            // Kiểm tra xem nhân viên có tồn tại không
            var existingPersonal = db.Personals.Find(id);
            if (existingPersonal == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin của nhân viên
            existingPersonal.First_Name = personal.First_Name;
            existingPersonal.Last_Name = personal.Last_Name;
            existingPersonal.Middle_Initial = personal.Middle_Initial;
            existingPersonal.Address1 = personal.Address1;
            existingPersonal.Address2 = personal.Address2;
            existingPersonal.City = personal.City;
            existingPersonal.State = personal.State;
            existingPersonal.Zip = personal.Zip;
            existingPersonal.Email = personal.Email;
            existingPersonal.Phone_Number = personal.Phone_Number;
            existingPersonal.Social_Security_Number = personal.Social_Security_Number;
            existingPersonal.Drivers_License = personal.Drivers_License;
            existingPersonal.Marital_Status = personal.Marital_Status;
            existingPersonal.Gender = personal.Gender;
            existingPersonal.Shareholder_Status = personal.Shareholder_Status;
            existingPersonal.Benefit_Plans = personal.Benefit_Plans;
            existingPersonal.Ethnicity = personal.Ethnicity;

            try
            {
                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }
            catch (Exception)
            {
                return InternalServerError();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Delete(decimal id)
        {
            // có thể xóa được nhưng phải xóa hết các khóa ngoại trước khi xóa ở bảng personal thì ta cần xóa ở bảng job_history, employment, emergency_contacts
            try
            {
                // Tìm đối tượng Personal trong cơ sở dữ liệu với ID được cung cấp
                Personal personal = db.Personals.Find(id);

                if (personal == null)
                {
                    // Trả về mã 404 Not Found nếu không tìm thấy đối tượng Personal với ID tương ứng
                    return Ok(new { success = false, data = "Không tìm thấy đối tượng để xóa" });
                }

                // Xóa đối tượng Personal khỏi cơ sở dữ liệu
                db.Personals.Remove(personal);
                db.SaveChanges();

                // Trả về mã 204 No Content để chỉ ra rằng hoạt động xóa thành công
                return Ok(new { success = true, data = personal });
            }
            catch (Exception ex)
            {
                // Trả về mã lỗi 500 Internal Server Error nếu có lỗi xảy ra trong quá trình xóa
                return Ok(new { success = false, data = ex.Message });
            }
        }
    }
}
