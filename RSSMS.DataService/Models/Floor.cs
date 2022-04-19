using System;
using System.Collections.Generic;

#nullable disable

namespace RSSMS.DataService.Models
{
    public partial class Floor
    {
        public Floor()
        {
            Exports = new HashSet<Export>();
            Imports = new HashSet<Import>();
            TransferFloorFroms = new HashSet<Transfer>();
            TransferFloorTos = new HashSet<Transfer>();
        }

        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Space Space { get; set; }
        public virtual ICollection<Export> Exports { get; set; }
        public virtual ICollection<Import> Imports { get; set; }
        public virtual ICollection<Transfer> TransferFloorFroms { get; set; }
        public virtual ICollection<Transfer> TransferFloorTos { get; set; }
    }
}
