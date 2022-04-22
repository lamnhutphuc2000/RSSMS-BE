using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RSSMS.DataService.ViewModels.Requests
{
    public class RequestScheduleViewModel : IComparable<RequestScheduleViewModel>
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public string OrderName { get; set; }
        public Guid? UserId { get; set; }
        public int? Type { get; set; }
        public int? TypeOrder { get; set; }
        public int? Status { get; set; }
        public bool? IsCustomerDelivery { get; set; }
        public string DeliveryStaffName { get; set; }
        public string DeliveryStaffPhone { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public Guid? StorageId { get; set; }
        public string StorageName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReturnAddress { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string ReturnTime { get; set; }
        public string CancelReason { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int CompareTo([AllowNull] RequestScheduleViewModel other)
        {
            IDictionary<string, int> deliveryString = new Dictionary<string,int>();

            deliveryString.Add("8am - 10am", 0);
            deliveryString.Add("10am - 12pm", 1);
            deliveryString.Add("12pm - 2pm", 2);
            deliveryString.Add("2pm - 4pm", 3);
            deliveryString.Add("4pm - 6pm", 4);

            if (deliveryString[DeliveryTime] > deliveryString[other.DeliveryTime])
                return 1;
            if (deliveryString[DeliveryTime] == deliveryString[other.DeliveryTime])
                return 0;
            return -1;
        }
    }
}
