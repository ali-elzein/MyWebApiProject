using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace MyWebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComplexTypeController : ControllerBase
    {
        public class StudentInfo
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public PrivateInfo[] Confidential { get; set;  }
        }
        public class PrivateInfo
        {
            public string SecretID { get; set; }
            public int Salary { get; set; }
        }
        [HttpPost("processstudents")]
        public IActionResult ProcessStudents (
            [FromBody] List<StudentInfo> students
        )
        {
            return Ok(new
            {
                Students = students,
                Message = "Students processed successfully"
            });
        }

        [HttpGet]
        public IActionResult Get(
            [FromQuery] List<object> items,
            [FromQuery] Dictionary<string, string> metadata,
            [FromBody] List<StudentInfo> myDict
        )
        {
            // Handle empty inputs
            // if ((items == null || items.Count == 0) && 
            //     (metadata == null || metadata.Count == 0))
            // {
            //     return BadRequest("Please provide at least one 'items' or 'metadata' parameter");
            // }

            return Ok(new
            {
                // Items = items ?? new List<string>(),
                // Metadata = metadata ?? new Dictionary<string, string>(),
                BodyData = myDict ?? new List<StudentInfo>(),
            });
        }
    }
}