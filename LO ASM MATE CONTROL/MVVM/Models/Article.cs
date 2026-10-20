using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LO_ASM_MATE_CONTROL.MVVM.Models
{
    public class Article
    {
        [Key]
        public string ArticleUid { get; set; }

        public string Designation { get; set; }

        public string Store { get; set; }

        public string Family { get; set; }

        public string Packaging { get; set; }

        public string ManageStock { get; set; }

        public string SupplierReference { get; set; }

        public int GpaoQuantity { get; set; }

        public override string ToString()
        {
            return $"ArticleUid: {ArticleUid}, Designation: {Designation}, Store: {Store}, Family: {Family}, " +
                   $"Packaging: {Packaging}, ManageStock: {ManageStock}, SupplierReference: {SupplierReference}, GpaoQuantity: {GpaoQuantity}";
        }
    }
}
