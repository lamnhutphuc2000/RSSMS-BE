using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.Collections.Generic;

namespace RSSMS.DataService.ViewModels.JWT
{
    public class TokenViewModel
    {
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public double ExpiresIn { get; set; }
        public string LocalId { get; set; }
        public string TokenType { get; set; }
        public Guid UserId { get; set; }
        public Guid? StorageId { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string RoleName { get; set; }
        public string Phone { get; set; }
        public virtual string ImageUrl { get; set; }
        public virtual ICollection<StaffAssignStorageViewModel> StaffAssignStorages { get; set; }

    }
}
