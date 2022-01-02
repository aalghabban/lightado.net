using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightADO.Test
{
    public class Category
    {
        [ColumnName("CategoryId")]
        public int Id { get; set; }

        [ColumnName("CategoryName")]
        public string Name { get; set; }        

        public List<Category> GetListOfCategory()
        {
            return new Query().ExecuteToListOfObject<Category>("select * from categories", System.Data.CommandType.Text);
        }

    }
}
