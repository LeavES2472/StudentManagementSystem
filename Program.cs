using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using static System.Console;

namespace 学生成绩管理系统
{
    public delegate void SceneFunction();
    class Student
    {
        public int Id { get; set; } = 0;
        public string? Name { get; set; }
        public double Score { get; set; }

    }

    class StudentManager
    {
        private int _nextId = 1; // 从1开始分配ID
        Dictionary<int, Student> students = new Dictionary<int, Student>();

        //数据访问方法
        //获取students的值转换成列表
        public IEnumerable<Student> GetAllStudents() => students.Values;
        public List<Student> FindStudentsByName(string name) =>
                students.Values
                .Where(kv => kv.Name != null && kv.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        public bool AddStudent(string name, double score)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            // 创建新学生对象（每个学生独立实例）
            Student student = new Student
            {
                Id = _nextId++,   // 使用递增ID
                Name = name,
                Score = score
            };

            //添加到字典，并递增ID
            students.Add(student.Id, student);
            return true;

        }

        //按 ID 删除
        public bool DeleteStudent(int id)
        {
            return students.Remove(id);
        }
        //按姓名删除（只删第一个匹配的
        public bool DeleteStudent(string name)
        {
            var matched = FindStudentsByName(name);
            if (matched.Count == 0)
                return false;
            // 如果有多个，只删第一个
            var target = matched.First();
            return students.Remove(target.Id);
        }
        //修改学生信息
        public bool UpdateStudent(int id, string newName, double newScore)
        {
            if (!students.ContainsKey(id))
                return false;
            var s = students[id];
            s.Name = newName;
            s.Score = newScore;
            return true;
        }
        //按姓名修改（只改第一个匹配的）
        public bool UpdateStudent(string name, string newName, double newScore)
        {
            var matched = FindStudentsByName(name);
            if (matched.Count == 0)
                return false;
            var s = matched.First();
            s.Name = newName;
            s.Score = newScore;
            return true;
        }
        public Student? GetStudentById(int id) => students.GetValueOrDefault(id);
        public Student? GetStudentByName(string name)
        {
            var matched = FindStudentsByName(name);
            return matched.Count == 0 ? null : matched.First();
        }
        public string GetSortedMaxToMin()
        {
            if (students.Count == 0) return "暂无学生信息";
            //OrderByDescending 不会修改原有的 students 字典,只是创建一个新的、排序好的“查询对象”（IOrderedEnumerable）
            var sorted = students.OrderByDescending(kv => kv.Value.Score);
            var sb = new System.Text.StringBuilder();
            //追加字符串
            sb.AppendLine("按成绩从高到低排序：");
            foreach (var kv in sorted)
                sb.AppendLine($"ID: {kv.Key}, 姓名: {kv.Value.Name}, 成绩: {kv.Value.Score}");
            return sb.ToString();
        }

        public string GetSortedMinToMax()
        {
            if (students.Count == 0) return "暂无学生信息";
            //OrderBy 不会修改原有的 students 字典
            var sorted = students.OrderBy(kv => kv.Value.Score);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("按成绩从低到高排序：");
            foreach (var kv in sorted)
                sb.AppendLine($"ID: {kv.Key}, 姓名: {kv.Value.Name}, 成绩: {kv.Value.Score}");
            return sb.ToString();
        }
    }
    class Program
    {
        static StudentManager manager = new StudentManager();
        public static void MainMenu()
        {
            WriteLine("********************");
            WriteLine("**学生成绩管理系统**");
            WriteLine("*****1.添加学生*****");
            WriteLine("*****2.删除学生*****");
            WriteLine("*****3.修改信息*****");
            WriteLine("*****4.查询信息*****");
            WriteLine("*****5.显示全部*****");
            WriteLine("*****6.排序显示*****");
            WriteLine("*****7.退出系统*****");
            WriteLine("********************");
        }
        public static void SortMenu()
        {
            WriteLine("********************");
            WriteLine("*****1.从大到小*****");
            WriteLine("*****2.从小到大*****");
            WriteLine("****3.返回主菜单****");
            WriteLine("********************");
        }
        public static void MenuChoice()
        {
            //将选择与循环分离
            Dictionary<int, SceneFunction> chooseScene = new Dictionary<int, SceneFunction>()
            {
                {1,AddStudentUI },
                {2,DeleteStudentUI },
                {3,UpdateStudentUI  },
                {4,FindStudentUI  },
                {5,DisplayAllUI  },
                {6,SortMenuChoice }, 
                {7,() => Environment.Exit(0)  },
            };

            WriteLine("请输入数字进行选择:");
            while (true)
            {
                string? inputChoice = ReadLine();

                if (int.TryParse(inputChoice, out int menuChoice))
                {
                    if (chooseScene.ContainsKey(menuChoice))
                    {
                        chooseScene[menuChoice].Invoke();// 执行对应的方法
                        // 执行完后，重新显示菜单
                        MainMenu();
                        WriteLine("请继续选择:");
                    }
                    else
                    {
                        WriteLine("输入的数字不在有效范围（1~7），请重新输入:");
                    }
                }
                else
                {
                    WriteLine("输入格式有误，请重新输入");
                }
            }

        }
        public static void SortMenuChoice()
        {
            while (true)
            {
                SortMenu(); // 显示排序子菜单
                WriteLine("请输入数字进行选择:");
                string? input = ReadLine();

                if (int.TryParse(input, out int choice))
                {
                    switch (choice)
                    {
                        case 1:
                            WriteLine(manager.GetSortedMaxToMin());
                            break;
                        case 2:
                            WriteLine(manager.GetSortedMinToMax());
                            break;
                        case 3:
                            return; // 返回主菜单
                        default:
                            WriteLine("输入无效，请重新输入");
                            break;
                    }
                }
                else
                {
                    WriteLine("输入格式有误，请重新输入");
                }
                WriteLine(); // 空行分隔
            }
        }
        public static void AddStudentUI()
        {
            // 1. 获取姓名
            WriteLine("请输入学生姓名（不能为空）：");
            string? name = ReadLine();

            // 2. 获取成绩
            WriteLine("请输入学生成绩（数字）：");
            string? scoreInput = ReadLine();
            if (!double.TryParse(scoreInput, out double score))
            {
                WriteLine("成绩格式错误，请输入有效的数字");
                return;
            }

            if (manager.AddStudent(name!, score))
                WriteLine($"学生 {name} 添加成功！");
            else
                WriteLine("添加失败，姓名不能为空。");

        }
        public static void DeleteStudentUI()
        {
            WriteLine("请输入要删除的学生ID或姓名：");
            string? input = ReadLine();
            if (int.TryParse(input, out int id))
            {
                if (manager.DeleteStudent(id))
                    WriteLine("删除成功");
                else
                    WriteLine("未找到该ID的学生");
            }
            else
            {
                if (manager.DeleteStudent(input!))
                    WriteLine("删除成功");
                else
                    WriteLine("未找到该姓名的学生");
            }
        }
        public static void UpdateStudentUI()
        {
            WriteLine("请输入要修改的学生ID或姓名：");
            string? input = ReadLine();
            WriteLine("请输入修改后的学生姓名：");
            string? name = ReadLine();
            WriteLine("请输入修改后的学生成绩：");
            string? score = ReadLine();
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(name))
            {
                WriteLine("姓名不能为空");
                return;
            }
            if (!double.TryParse(score, out double newScore))
            {
                WriteLine("成绩格式错误，请输入有效的数字");
                return;
            }
            if (int.TryParse(input, out int id))
            {
                if (manager.UpdateStudent(id, name!, newScore))
                    WriteLine("修改成功");
                else
                    WriteLine("未找到该ID的学生");
            }
            else
            {
                if (manager.UpdateStudent(input!, name!, newScore))
                    WriteLine("修改成功");
                else
                    WriteLine("未找到该姓名的学生");
            }
        }
        public static void FindStudentUI()
        {
            WriteLine("请输入你要查询的学生姓名或ID");
            string? inputNameOrId = ReadLine();

            if (int.TryParse(inputNameOrId, out int id))
            {
                var student = manager.GetStudentById(id);
                if (student != null)
                    WriteLine($"查找到的学生ID:{student.Id},姓名:{student.Name},成绩:{student.Score}");
                else
                    WriteLine("未查找到学生信息");
            }
            else
            {
                var student = manager.GetStudentByName(inputNameOrId!);
                if (student != null)
                    WriteLine($"查找到的学生ID:{student.Id},姓名:{student.Name},成绩:{student.Score}");
                else
                    WriteLine("未查找到学生信息");
            }

        }
        public static void DisplayAllUI()
        {
            var all = manager.GetAllStudents().ToList();
            if (all.Count == 0)
            {
                WriteLine("暂无学生信息");
                return;
            }
            foreach (var s in all)
            {
                WriteLine($"ID: {s.Id}, 姓名: {s.Name}, 成绩: {s.Score}");
            }
        }
        static void Main(string[] args)
        {
            MainMenu();
            MenuChoice();
        }

    }
}