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

            // Initialize with some sample claims
            InitializeSampleClaims();
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

        private static void InitializeSampleClaims()
        {
            // Add some sample claims for testing
            _claims.Add(new Claim
            {
                ClaimId = _nextClaimId++,
                LecturerId = 2, // John Lecturer
                TotalHours = 25,
                StoredHourlyRate = 350,
                StoredTotalAmount = 8750,
                Month = "2025-11",
                Status = "Verified",
                VerifiedByCoordinatorId = 3, // Sarah Coordinator
                VerifiedDate = DateTime.Now,
                SubmittedDate = DateTime.Now.AddDays(-2)
            });

            _claims.Add(new Claim
            {
                ClaimId = _nextClaimId++,
                LecturerId = 2, // John Lecturer
                TotalHours = 30,
                StoredHourlyRate = 350,
                StoredTotalAmount = 10500,
                Month = "2025-10",
                Status = "Approved",
                VerifiedByCoordinatorId = 3, // Sarah Coordinator
                ApprovedByManagerId = 4, // Michael Manager
                VerifiedDate = DateTime.Now.AddDays(-10),
                ApprovedDate = DateTime.Now.AddDays(-5),
                SubmittedDate = DateTime.Now.AddDays(-15)
            });
        }

        // User methods
        public static List<User> GetUsers() => _users;
        public static void AddUser(User user) => _users.Add(user);
        public static User GetUserById(int id) => _users.FirstOrDefault(u => u.UserId == id);

        // Claim methods  
        public static List<Claim> GetClaims()
        {
            var users = GetUsers();

            // Load related data for all claims
            foreach (var claim in _claims)
            {
                // Load Lecturer (LecturerId is NOT nullable, so no HasValue check needed)
                claim.Lecturer = users.FirstOrDefault(u => u.UserId == claim.LecturerId);

                // Load VerifiedByCoordinator (this IS nullable)
                if (claim.VerifiedByCoordinatorId.HasValue)
                {
                    claim.VerifiedByCoordinator = users.FirstOrDefault(u => u.UserId == claim.VerifiedByCoordinatorId.Value);
                }

                // Load ApprovedByManager (this IS nullable)
                if (claim.ApprovedByManagerId.HasValue)
                {
                    claim.ApprovedByManager = users.FirstOrDefault(u => u.UserId == claim.ApprovedByManagerId.Value);
                }
            }

            return _claims;
        }

        public static void AddClaim(Claim claim)
        {
            claim.ClaimId = _nextClaimId++;
            _claims.Add(claim);
        }

        public static void UpdateClaim(Claim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.ClaimId == updatedClaim.ClaimId);
            if (existingClaim != null)
            {
                // Update all properties
                existingClaim.TotalHours = updatedClaim.TotalHours;
                existingClaim.StoredHourlyRate = updatedClaim.StoredHourlyRate;
                existingClaim.StoredTotalAmount = updatedClaim.StoredTotalAmount;
                existingClaim.Notes = updatedClaim.Notes;
                existingClaim.Month = updatedClaim.Month;
                existingClaim.Status = updatedClaim.Status;
                existingClaim.VerifiedByCoordinatorId = updatedClaim.VerifiedByCoordinatorId;
                existingClaim.ApprovedByManagerId = updatedClaim.ApprovedByManagerId;
                existingClaim.VerifiedDate = updatedClaim.VerifiedDate;
                existingClaim.ApprovedDate = updatedClaim.ApprovedDate;
                existingClaim.DocumentPath = updatedClaim.DocumentPath;
                existingClaim.DocumentOriginalName = updatedClaim.DocumentOriginalName;
            }
        }

        public static Claim GetClaimById(int id)
        {
            return GetClaims().FirstOrDefault(c => c.ClaimId == id);
        }
    }
}