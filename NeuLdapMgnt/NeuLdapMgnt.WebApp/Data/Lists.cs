using NeuLdapMgnt.WebApp.Model;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Data
{
    public static class Lists
    {
        public static List<Student> Students { get; set; } = new();

        private static List<Teacher> Teachers { get; set; } = new();

        private static List<AdminModel> Admins { get; set; } = new();

        public static List<Student> GetStudents() => Students;
        public static List<Teacher> GetTeachers() => Teachers;
        public static List<AdminModel> GetAdmins() => Admins;
    }
}
