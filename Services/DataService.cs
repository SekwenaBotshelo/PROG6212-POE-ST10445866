// Services/DataService.cs
using PROG6212_POE.Models;

namespace PROG6212_POE.Services
{
    public static class DataService
    {
        private static List<User> _users = new List<User>();
        private static List<Claim> _claims = new List<Claim>();
        private static int _nextUserId = 1;
        private static int _nextClaimId = 101;

        static DataService()
        {
            // Initialize with default users
            InitializeDefaultUsers();
        }

        private static void InitializeDefaultUsers()
        {
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "HR",
                Surname = "Manager",
                Email = "hr@university.com",
                HourlyRate = 0,
                Role = "HR",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "John",
                Surname = "Lecturer",
                Email = "lecturer@university.com",
                HourlyRate = 350,
                Role = "Lecturer",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "Sarah",
                Surname = "Coordinator",
                Email = "coordinator@university.com",
                HourlyRate = 0,
                Role = "Coordinator",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "Michael",
                Surname = "Manager",
                Email = "manager@university.com",
                HourlyRate = 0,
                Role = "Manager",
                Password = "password123"
            });
        }

        // User methods
        public static List<User> GetUsers() => _users;
        public static void AddUser(User user) => _users.Add(user);
        public static User GetUserById(int id) => _users.FirstOrDefault(u => u.UserId == id);

        // Claim methods  
        public static List<Claim> GetClaims() => _claims;
        public static void AddClaim(Claim claim)
        {
            claim.ClaimId = _nextClaimId++;
            _claims.Add(claim);
        }
        public static void UpdateClaim(Claim claim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.ClaimId == claim.ClaimId);
            if (existingClaim != null)
            {
                var index = _claims.IndexOf(existingClaim);
                _claims[index] = claim;
            }
        }
        public static List<Claim> GetClaimsByLecturer(int lecturerId) =>
            _claims.Where(c => c.LecturerId == lecturerId).ToList();
    }
}