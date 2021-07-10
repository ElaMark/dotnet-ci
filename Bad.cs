using System;

namespace dotnet_ci
{
    public class MyClass
    {
        public void MyMethod()
        {
            string str = "";
            for (int i = 0; i < 10; i++)
            {
                str = str + i + "-";  // yuk
            }
            str="Select from table where t.thing like '%" + str + "%'";
        }
    }
}
