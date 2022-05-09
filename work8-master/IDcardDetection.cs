using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IDcardDetection
{
    class Program
    {
        static void Parseid(string id)
        {
          //  string pattern = @"^\d{17}(?:\d|X)$";
            string birth = id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();

            int[] arr_weight = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };     // 加权数组
            string[] id_last = { "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };   // 校验数组
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += arr_weight[i] * int.Parse(id[i].ToString());
            }
            int result = sum % 11;  // 实际校验位的值

           // if (Regex.IsMatch(id, pattern))                     // 18位格式检查
          //  {
                if (DateTime.TryParse(birth, out time))          // 出生日期检查
                {
                    if (id_last[result] == id[17].ToString())   // 校验位检查
                    {
                        Console.WriteLine("身份证号格式正确!");
                    }
                    else
                    {
                        Console.WriteLine("最后一位校验错误!");
                    }
                }
                else
                {
                    Console.WriteLine("出生日期验证失败!");
                }
            }
           // else
           // {
           //     Console.WriteLine("身份证号格式错误!");
           // }

      //  }
        static void Main(string[] args)
        {
            string id;
            string pattern = @"^\d{17}(?:\d|X)$";
            Console.WriteLine("请输入身份证号：");
            id = Console.ReadLine();
            if (Regex.IsMatch(id, pattern))    //验证身份证号格式
            { 
                Parseid(id);
               // Console.Read();   如果在这里read，当格式不正确的话不能输出下一行
            }
            else
            { 
                Console.WriteLine("身份证号格式错误!"); 
            }
            Console.Read();
        }
    }
}