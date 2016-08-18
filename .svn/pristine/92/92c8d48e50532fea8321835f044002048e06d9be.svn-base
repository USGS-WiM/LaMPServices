using System;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LaMPWeb.Models
{
    public class DataManListingModel
    {
        [Required]
        public decimal DataManagerID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Data Manager name")]
        public string DataManagerName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Organization")]
        public string OrganizationName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }

    public class DataManagerCreateModel
    {
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string USERNAME { get; set; }
        public Int32 ORGANIZATION_ID { get; set; }
        public Int32 ROLE_ID { get; set; }
        public string PHONE { get; set; }
        public string EMAIL { get; set; }
        [Required]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}