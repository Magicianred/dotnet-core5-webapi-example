using it.example.dotnetcore5.domain.Interfaces.Models;
using System;

#nullable disable

namespace it.example.dotnetcore5.dal.ef.mysql.EfModels
{
    public partial class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public int? CategoryId { get; set; }
        public string Author { get; set; }

        public virtual Category Category { get; set; }
    }
}
