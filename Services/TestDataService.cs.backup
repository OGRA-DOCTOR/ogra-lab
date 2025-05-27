using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public class TestDataService : ITestDataService
    {
        private readonly OgraLabDbContext _context;
        private readonly ILoggingService _loggingService;
        private readonly Random _random;

        private readonly string[] _maleFirstNames = { "أحمد", "محمد", "علي", "حسن", "خالد", "فيصل", "عبدالله", "سلمان", "فهد", "طلال", "نواف", "بندر", "سعد", "ماجد", "عمر", "يوسف", "إبراهيم", "عبدالعزيز", "منصور", "تركي" };
        private readonly string[] _femaleFirstNames = { "فاطمة", "عائشة", "خديجة", "مريم", "زينب", "سارة", "نورا", "ريم", "لينا", "دانا", "جود", "روان", "غلا", "شهد", "رغد", "أمل", "هدى", "نور", "لمى", "رنا" };
        private readonly string[] _lastNames = { "الأحمد", "المحمد", "العلي", "الحسن", "الخالد", "الفيصل", "العبدالله", "السلمان", "الفهد", "الطلال", "النواف", "البندر", "السعد", "الماجد", "العمر", "اليوسف", "الإبراهيم", "العبدالعزيز", "المنصور", "التركي" };
        
        private readonly string[] _doctors = { "د. أحمد السالم", "د. فاطمة النور", "د. محمد الراشد", "د. سارة الزهراني", "د. خالد العتيبي", "د. نورا القحطاني", "د. عبدالله الشهري", "د. مريم الدوسري", "د. فيصل المطيري", "د. ريم الحربي" };
        
        private readonly string[] _cities = { "الرياض", "جدة", "الدمام", "مكة المكرمة", "المدينة المنورة", "الطائف", "بريدة", "تبوك", "الخبر", "حائل", "الجبيل", "ينبع", "الأحساء", "القطيف", "خميس مشيط", "نجران", "جازان", "عرعر", "سكاكا", "أبها" };

        public TestDataService(OgraLabDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
            _random = new Random();
        }

        #region Data Creation

        public async Task<bool> CreateSamplePatientsAsync(int count = 50)
        {
            try
            {
                var existingPatients = await _context.Patients.CountAsync();
                if (existingPatients >= count)
                {
                    await _loggingService.LogInfoAsync($"يوجد بالفعل {existingPatients} مريض، لن يتم إنشاء المزيد", "TestData");
                    return true;
                }

                var patients = new List<Patient>();
                var usedPatientNumbers = new HashSet<string>();
                var usedNationalIds = new HashSet<string>();

                for (int i = 0; i < count; i++)
                {
                    var gender = _random.Next(2) == 0 ? "ذكر" : "أنثى";
                    var firstName = gender == "ذكر" 
                        ? _maleFirstNames[_random.Next(_maleFirstNames.Length)]
                        : _femaleFirstNames[_random.Next(_femaleFirstNames.Length)];
                    var lastName = _lastNames[_random.Next(_lastNames.Length)];

                    // Generate unique patient number
                    string patientNumber;
                    do
                    {
                        patientNumber = $"P{DateTime.Now.Year}{_random.Next(1000, 9999)}";
                    } while (usedPatientNumbers.Contains(patientNumber));
                    usedPatientNumbers.Add(patientNumber);

                    // Generate unique national ID
                    string nationalId;
                    do
                    {
                        nationalId = $"1{_random.Next(100000000, 999999999)}";
                    } while (usedNationalIds.Contains(nationalId));
                    usedNationalIds.Add(nationalId);

                    var patient = new Patient
                    {
                        PatientNumber = patientNumber,
                        FirstName = firstName,
                        LastName = lastName,
                        FullName = $"{firstName} {lastName}",
                        DateOfBirth = DateTime.Now.AddYears(-_random.Next(18, 80)).AddDays(-_random.Next(365)),
                        Gender = gender,
                        NationalId = nationalId,
                        Phone = $"05{_random.Next(10000000, 99999999)}",
                        Email = $"{firstName.Replace(" ", "").ToLower()}.{lastName.Replace(" ", "").ToLower()}@email.com",
                        Address = $"{_cities[_random.Next(_cities.Length)]} - حي {_random.Next(1, 20)}",
                        EmergencyContact = $"05{_random.Next(10000000, 99999999)}",
                        EmergencyContactName = $"{_maleFirstNames[_random.Next(_maleFirstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}",
                        EmergencyContactRelation = _random.Next(3) switch
                        {
                            0 => "أب",
                            1 => "زوج",
                            _ => "أخ"
                        },
                        MedicalHistory = _random.Next(4) switch
                        {
                            0 => "السكري",
                            1 => "ضغط الدم",
                            2 => "السكري وضغط الدم",
                            _ => "لا يوجد"
                        },
                        CurrentMedications = _random.Next(3) switch
                        {
                            0 => "دواء السكري",
                            1 => "دواء الضغط",
                            _ => "لا يوجد"
                        },
                        Allergies = _random.Next(4) switch
                        {
                            0 => "البنسلين",
                            1 => "الأسبرين",
                            2 => "لا يوجد",
                            _ => "حساسية موسمية"
                        },
                        IsActive = true,
                        CreatedDate = DateTime.Now.AddDays(-_random.Next(365)),
                        CreatedBy = "TestDataService"
                    };

                    patients.Add(patient);
                }

                await _context.Patients.AddRangeAsync(patients);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {count} مريض تجريبي", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء المرضى التجريبيين", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateSampleTestTypesAsync()
        {
            try
            {
                var existingTestTypes = await _context.TestTypes.CountAsync();
                if (existingTestTypes > 10) // Already has sample data
                {
                    await _loggingService.LogInfoAsync("أنواع التحاليل موجودة بالفعل", "TestData");
                    return true;
                }

                var testTypes = new List<TestType>
                {
                    // Blood Chemistry
                    new TestType { TestCode = "ALT", TestName = "إنزيم الكبد ALT", Category = "كيمياء", Unit = "U/L", MinNormalValue = 7, MaxNormalValue = 56, NormalRange = "7-56 U/L", Price = 30, PreparationTime = 5, ResultTime = 4, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "AST", TestName = "إنزيم الكبد AST", Category = "كيمياء", Unit = "U/L", MinNormalValue = 10, MaxNormalValue = 40, NormalRange = "10-40 U/L", Price = 30, PreparationTime = 5, ResultTime = 4, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "BILI", TestName = "البيليروبين", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 0.3m, MaxNormalValue = 1.2m, NormalRange = "0.3-1.2 mg/dL", Price = 25, PreparationTime = 5, ResultTime = 3, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "UREA", TestName = "اليوريا", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 15, MaxNormalValue = 45, NormalRange = "15-45 mg/dL", Price = 20, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "CREAT", TestName = "الكرياتينين", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 0.7m, MaxNormalValue = 1.3m, NormalRange = "0.7-1.3 mg/dL", Price = 25, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    
                    // Lipid Profile
                    new TestType { TestCode = "TG", TestName = "الدهون الثلاثية", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 0, MaxNormalValue = 150, NormalRange = "أقل من 150 mg/dL", Price = 35, PreparationTime = 5, ResultTime = 3, RequiresFasting = true, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "HDL", TestName = "الكوليسترول المفيد HDL", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 40, MaxNormalValue = 60, NormalRange = "أكثر من 40 mg/dL", Price = 40, PreparationTime = 5, ResultTime = 3, RequiresFasting = true, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "LDL", TestName = "الكوليسترول الضار LDL", Category = "كيمياء", Unit = "mg/dL", MinNormalValue = 0, MaxNormalValue = 100, NormalRange = "أقل من 100 mg/dL", Price = 40, PreparationTime = 5, ResultTime = 3, RequiresFasting = true, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    
                    // Hormones
                    new TestType { TestCode = "T3", TestName = "هرمون الغدة الدرقية T3", Category = "هرمونات", Unit = "ng/dL", MinNormalValue = 80, MaxNormalValue = 200, NormalRange = "80-200 ng/dL", Price = 80, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "T4", TestName = "هرمون الغدة الدرقية T4", Category = "هرمونات", Unit = "μg/dL", MinNormalValue = 5, MaxNormalValue = 12, NormalRange = "5-12 μg/dL", Price = 80, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "TESTO", TestName = "هرمون التستوستيرون", Category = "هرمونات", Unit = "ng/mL", MinNormalValue = 3, MaxNormalValue = 10, NormalRange = "3-10 ng/mL", Price = 100, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "ESTRO", TestName = "هرمون الاستروجين", Category = "هرمونات", Unit = "pg/mL", MinNormalValue = 30, MaxNormalValue = 400, NormalRange = "30-400 pg/mL", Price = 100, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    
                    // Blood Count
                    new TestType { TestCode = "WBC", TestName = "خلايا الدم البيضاء", Category = "دم", Unit = "×10³/μL", MinNormalValue = 4, MaxNormalValue = 11, NormalRange = "4-11 ×10³/μL", Price = 30, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "RBC", TestName = "خلايا الدم الحمراء", Category = "دم", Unit = "×10⁶/μL", MinNormalValue = 4.2m, MaxNormalValue = 5.4m, NormalRange = "4.2-5.4 ×10⁶/μL", Price = 30, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "HGB", TestName = "الهيموجلوبين", Category = "دم", Unit = "g/dL", MinNormalValue = 12, MaxNormalValue = 16, NormalRange = "12-16 g/dL", Price = 25, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "PLT", TestName = "الصفائح الدموية", Category = "دم", Unit = "×10³/μL", MinNormalValue = 150, MaxNormalValue = 450, NormalRange = "150-450 ×10³/μL", Price = 30, PreparationTime = 5, ResultTime = 2, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    
                    // Vitamins
                    new TestType { TestCode = "VITD", TestName = "فيتامين د", Category = "فيتامينات", Unit = "ng/mL", MinNormalValue = 30, MaxNormalValue = 100, NormalRange = "30-100 ng/mL", Price = 120, PreparationTime = 10, ResultTime = 48, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "VITB12", TestName = "فيتامين ب12", Category = "فيتامينات", Unit = "pg/mL", MinNormalValue = 200, MaxNormalValue = 900, NormalRange = "200-900 pg/mL", Price = 100, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "FOLIC", TestName = "حمض الفوليك", Category = "فيتامينات", Unit = "ng/mL", MinNormalValue = 3, MaxNormalValue = 17, NormalRange = "3-17 ng/mL", Price = 90, PreparationTime = 10, ResultTime = 24, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    
                    // Tumor Markers
                    new TestType { TestCode = "PSA", TestName = "مؤشر البروستات PSA", Category = "أورام", Unit = "ng/mL", MinNormalValue = 0, MaxNormalValue = 4, NormalRange = "0-4 ng/mL", Price = 150, PreparationTime = 15, ResultTime = 48, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "CEA", TestName = "مؤشر الأورام CEA", Category = "أورام", Unit = "ng/mL", MinNormalValue = 0, MaxNormalValue = 5, NormalRange = "0-5 ng/mL", Price = 150, PreparationTime = 15, ResultTime = 48, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" },
                    new TestType { TestCode = "CA125", TestName = "مؤشر الأورام CA125", Category = "أورام", Unit = "U/mL", MinNormalValue = 0, MaxNormalValue = 35, NormalRange = "0-35 U/mL", Price = 150, PreparationTime = 15, ResultTime = 48, RequiresFasting = false, CreatedDate = DateTime.Now, CreatedBy = "TestDataService" }
                };

                await _context.TestTypes.AddRangeAsync(testTypes);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {testTypes.Count} نوع تحليل تجريبي", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء أنواع التحاليل التجريبية", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateSampleTestGroupsAsync()
        {
            try
            {
                var existingGroups = await _context.TestGroups.CountAsync();
                if (existingGroups > 5) // Already has sample data
                {
                    await _loggingService.LogInfoAsync("مجموعات التحاليل موجودة بالفعل", "TestData");
                    return true;
                }

                var groups = new List<TestGroup>
                {
                    new TestGroup
                    {
                        GroupCode = "LIPID",
                        GroupName = "ملف الدهون الشامل",
                        Description = "فحص شامل للكوليسترول والدهون الثلاثية",
                        GroupPrice = 200.00m,
                        DiscountPercentage = 20.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    },
                    new TestGroup
                    {
                        GroupCode = "THYROID",
                        GroupName = "ملف الغدة الدرقية",
                        Description = "فحص شامل لوظائف الغدة الدرقية",
                        GroupPrice = 300.00m,
                        DiscountPercentage = 25.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    },
                    new TestGroup
                    {
                        GroupCode = "LIVER",
                        GroupName = "ملف وظائف الكبد",
                        Description = "فحص شامل لوظائف الكبد والإنزيمات",
                        GroupPrice = 180.00m,
                        DiscountPercentage = 15.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    },
                    new TestGroup
                    {
                        GroupCode = "KIDNEY",
                        GroupName = "ملف وظائف الكلى",
                        Description = "فحص شامل لوظائف الكلى",
                        GroupPrice = 120.00m,
                        DiscountPercentage = 10.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    },
                    new TestGroup
                    {
                        GroupCode = "VITAMINS",
                        GroupName = "ملف الفيتامينات",
                        Description = "فحص مستويات الفيتامينات الأساسية",
                        GroupPrice = 400.00m,
                        DiscountPercentage = 30.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    },
                    new TestGroup
                    {
                        GroupCode = "CANCER",
                        GroupName = "ملف مؤشرات الأورام",
                        Description = "فحص مؤشرات الأورام الرئيسية",
                        GroupPrice = 500.00m,
                        DiscountPercentage = 25.00m,
                        CreatedDate = DateTime.Now,
                        CreatedBy = "TestDataService"
                    }
                };

                await _context.TestGroups.AddRangeAsync(groups);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {groups.Count} مجموعة تحاليل تجريبية", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء مجموعات التحاليل التجريبية", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateSampleUsersAsync()
        {
            try
            {
                var existingUsers = await _context.Users.CountAsync();
                if (existingUsers > 2) // Already has sample data beyond the default Manager user
                {
                    await _loggingService.LogInfoAsync("المستخدمون موجودون بالفعل", "TestData");
                    return true;
                }

                var users = new List<User>
                {
                    new User
                    {
                        Username = "AdminUser",
                        FullName = "أحمد الإداري",
                        Email = "admin@ogralab.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "AdminUser",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        PhoneNumber = "0501234568",
                        CreatedBy = "TestDataService"
                    },
                    new User
                    {
                        Username = "LabTech1",
                        FullName = "فاطمة المختبر",
                        Email = "fatima@ogralab.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("lab123"),
                        Role = "RegularUser",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        PhoneNumber = "0501234569",
                        CreatedBy = "TestDataService"
                    },
                    new User
                    {
                        Username = "LabTech2",
                        FullName = "محمد التقني",
                        Email = "mohammed@ogralab.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("lab123"),
                        Role = "RegularUser",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        PhoneNumber = "0501234570",
                        CreatedBy = "TestDataService"
                    },
                    new User
                    {
                        Username = "Supervisor",
                        FullName = "سارة المشرفة",
                        Email = "sarah@ogralab.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("super123"),
                        Role = "AdminUser",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        PhoneNumber = "0501234571",
                        CreatedBy = "TestDataService"
                    }
                };

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {users.Count} مستخدم تجريبي", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء المستخدمين التجريبيين", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateSamplePatientTestsAsync(int count = 100)
        {
            try
            {
                var patients = await _context.Patients.ToListAsync();
                var testTypes = await _context.TestTypes.ToListAsync();
                
                if (!patients.Any() || !testTypes.Any())
                {
                    await _loggingService.LogWarningAsync("لا توجد مرضى أو أنواع تحاليل لإنشاء تحاليل تجريبية", "TestData");
                    return false;
                }

                var existingTests = await _context.PatientTests.CountAsync();
                if (existingTests >= count)
                {
                    await _loggingService.LogInfoAsync($"يوجد بالفعل {existingTests} تحليل مريض", "TestData");
                    return true;
                }

                var patientTests = new List<PatientTest>();
                var usedOrderNumbers = new HashSet<string>();

                for (int i = 0; i < count; i++)
                {
                    var patient = patients[_random.Next(patients.Count)];
                    var testType = testTypes[_random.Next(testTypes.Count)];
                    
                    // Generate unique order number
                    string orderNumber;
                    do
                    {
                        orderNumber = $"ORD{DateTime.Now.Year}{_random.Next(100000, 999999)}";
                    } while (usedOrderNumbers.Contains(orderNumber));
                    usedOrderNumbers.Add(orderNumber);

                    var orderDate = DateTime.Now.AddDays(-_random.Next(90)); // Orders from last 90 days
                    var status = _random.Next(10) switch
                    {
                        0 => "Pending",
                        1 => "InProgress", 
                        _ => "Completed"
                    };

                    var patientTest = new PatientTest
                    {
                        OrderNumber = orderNumber,
                        PatientId = patient.PatientId,
                        TestTypeId = testType.TestTypeId,
                        OrderDate = orderDate,
                        DoctorName = _doctors[_random.Next(_doctors.Length)],
                        Priority = _random.Next(3) switch
                        {
                            0 => "منخفضة",
                            1 => "متوسطة",
                            _ => "عالية"
                        },
                        Status = status,
                        TotalAmount = testType.Price,
                        PaidAmount = testType.Price * (_random.Next(2) == 0 ? 1 : 0.5m), // Some partially paid
                        DiscountPercentage = _random.Next(4) == 0 ? _random.Next(5, 20) : 0, // Some with discount
                        Notes = _random.Next(3) == 0 ? "ملاحظات تجريبية" : "",
                        CreatedDate = orderDate,
                        CreatedBy = "TestDataService"
                    };

                    if (status == "Completed")
                    {
                        patientTest.CompletedDate = orderDate.AddHours(_random.Next(2, 48));
                        patientTest.CompletedBy = "LabTech1";
                    }

                    patientTests.Add(patientTest);
                }

                await _context.PatientTests.AddRangeAsync(patientTests);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {count} تحليل مريض تجريبي", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء تحاليل المرضى التجريبية", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateSampleTestResultsAsync()
        {
            try
            {
                var completedTests = await _context.PatientTests
                    .Include(pt => pt.TestType)
                    .Where(pt => pt.Status == "Completed")
                    .ToListAsync();

                if (!completedTests.Any())
                {
                    await _loggingService.LogWarningAsync("لا توجد تحاليل مكتملة لإنشاء نتائج", "TestData");
                    return false;
                }

                var existingResults = await _context.TestResults.CountAsync();
                if (existingResults > 0)
                {
                    await _loggingService.LogInfoAsync("النتائج موجودة بالفعل", "TestData");
                    return true;
                }

                var testResults = new List<TestResult>();

                foreach (var patientTest in completedTests)
                {
                    var testResult = new TestResult
                    {
                        PatientTestId = patientTest.PatientTestId,
                        TestTypeId = patientTest.TestTypeId,
                        TestDate = patientTest.OrderDate.Value,
                        ResultDate = patientTest.CompletedDate ?? patientTest.OrderDate.Value.AddHours(24),
                        CreatedBy = "TestDataService",
                        CreatedDate = patientTest.CompletedDate ?? DateTime.Now
                    };

                    // Generate realistic values based on test type
                    var testType = patientTest.TestType;
                    if (testType.MinNormalValue.HasValue && testType.MaxNormalValue.HasValue)
                    {
                        // Generate values within and outside normal range
                        var isNormal = _random.Next(4) != 0; // 75% normal values
                        
                        if (isNormal)
                        {
                            var range = testType.MaxNormalValue.Value - testType.MinNormalValue.Value;
                            testResult.NumericValue = testType.MinNormalValue.Value + 
                                (decimal)(_random.NextDouble() * (double)range);
                        }
                        else
                        {
                            // Generate abnormal values
                            var isHigh = _random.Next(2) == 0;
                            if (isHigh)
                            {
                                testResult.NumericValue = testType.MaxNormalValue.Value * 
                                    (decimal)(1.1 + _random.NextDouble() * 0.5); // 10-60% above normal
                            }
                            else
                            {
                                testResult.NumericValue = testType.MinNormalValue.Value * 
                                    (decimal)(0.5 + _random.NextDouble() * 0.4); // 50-90% of normal
                            }
                        }

                        testResult.TextValue = testResult.NumericValue?.ToString("F2");
                        testResult.IsAbnormal = !isNormal;
                    }
                    else
                    {
                        // For tests without numeric ranges, use text values
                        testResult.TextValue = _random.Next(4) switch
                        {
                            0 => "طبيعي",
                            1 => "مرتفع قليلاً",
                            2 => "منخفض قليلاً",
                            _ => "طبيعي"
                        };
                        testResult.IsAbnormal = testResult.TextValue != "طبيعي";
                    }

                    testResult.Unit = testType.Unit;
                    testResult.ReferenceRange = testType.NormalRange;
                    
                    if (testResult.IsAbnormal)
                    {
                        testResult.Comment = "قيمة خارج النطاق الطبيعي - يُنصح بمراجعة الطبيب";
                    }

                    testResults.Add(testResult);
                }

                await _context.TestResults.AddRangeAsync(testResults);
                await _context.SaveChangesAsync();

                await _loggingService.LogInfoAsync($"تم إنشاء {testResults.Count} نتيجة تحليل تجريبية", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء نتائج التحاليل التجريبية", ex, "TestData");
                return false;
            }
        }

        #endregion

        #region Full Sample Data

        public async Task<bool> CreateFullSampleDataAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("بدء إنشاء البيانات التجريبية الكاملة", "TestData");

                // Create in order of dependencies
                var success = true;
                success &= await CreateSampleUsersAsync();
                success &= await CreateSampleTestTypesAsync();
                success &= await CreateSampleTestGroupsAsync();
                success &= await CreateSamplePatientsAsync(50);
                success &= await CreateSamplePatientTestsAsync(150);
                success &= await CreateSampleTestResultsAsync();

                if (success)
                {
                    await _loggingService.LogInfoAsync("تم إنشاء البيانات التجريبية الكاملة بنجاح", "TestData");
                }
                else
                {
                    await _loggingService.LogWarningAsync("فشل في إنشاء بعض البيانات التجريبية", "TestData");
                }

                return success;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء البيانات التجريبية الكاملة", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> ClearAllSampleDataAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("بدء مسح البيانات التجريبية", "TestData");

                // Delete in reverse order of dependencies
                await _context.TestResults.Where(tr => tr.CreatedBy == "TestDataService").ExecuteDeleteAsync();
                await _context.PatientTests.Where(pt => pt.CreatedBy == "TestDataService").ExecuteDeleteAsync();
                await _context.Patients.Where(p => p.CreatedBy == "TestDataService").ExecuteDeleteAsync();
                await _context.TestGroups.Where(tg => tg.CreatedBy == "TestDataService").ExecuteDeleteAsync();
                await _context.TestTypes.Where(tt => tt.CreatedBy == "TestDataService").ExecuteDeleteAsync();
                await _context.Users.Where(u => u.CreatedBy == "TestDataService").ExecuteDeleteAsync();

                await _loggingService.LogInfoAsync("تم مسح البيانات التجريبية بنجاح", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في مسح البيانات التجريبية", ex, "TestData");
                return false;
            }
        }

        #endregion

        #region Specific Scenarios

        public async Task<bool> CreateDemoScenarioAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إنشاء سيناريو العرض التوضيحي", "TestData");

                // Create a smaller, focused dataset for demos
                var success = true;
                success &= await CreateSampleTestTypesAsync();
                success &= await CreateSampleTestGroupsAsync();
                success &= await CreateSampleUsersAsync();
                success &= await CreateSamplePatientsAsync(10); // Fewer patients
                success &= await CreateSamplePatientTestsAsync(20); // Fewer tests
                success &= await CreateSampleTestResultsAsync();

                return success;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء سيناريو العرض التوضيحي", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreatePerformanceTestDataAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إنشاء بيانات اختبار الأداء", "TestData");

                // Create large dataset for performance testing
                var success = true;
                success &= await CreateSampleTestTypesAsync();
                success &= await CreateSampleTestGroupsAsync();
                success &= await CreateSampleUsersAsync();
                success &= await CreateSamplePatientsAsync(500); // Large number
                success &= await CreateSamplePatientTestsAsync(2000); // Large number
                success &= await CreateSampleTestResultsAsync();

                return success;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء بيانات اختبار الأداء", ex, "TestData");
                return false;
            }
        }

        public async Task<bool> CreateReportingTestDataAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إنشاء بيانات اختبار التقارير", "TestData");

                // Create focused dataset with completed tests for reporting
                var success = await CreateFullSampleDataAsync();
                
                if (success)
                {
                    // Ensure most tests are completed for reporting
                    var pendingTests = await _context.PatientTests
                        .Where(pt => pt.Status != "Completed")
                        .ToListAsync();

                    foreach (var test in pendingTests.Take(pendingTests.Count * 80 / 100)) // Complete 80%
                    {
                        test.Status = "Completed";
                        test.CompletedDate = test.OrderDate?.AddHours(_random.Next(2, 48));
                        test.CompletedBy = "LabTech1";
                    }

                    await _context.SaveChangesAsync();
                    await CreateSampleTestResultsAsync(); // Create results for new completed tests
                }

                return success;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء بيانات اختبار التقارير", ex, "TestData");
                return false;
            }
        }

        #endregion

        #region Data Validation

        public async Task<bool> ValidateDataIntegrityAsync()
        {
            try
            {
                var issues = new List<string>();

                // Check for orphaned records
                var orphanedResults = await _context.TestResults
                    .Where(tr => !_context.PatientTests.Any(pt => pt.PatientTestId == tr.PatientTestId))
                    .CountAsync();
                
                if (orphanedResults > 0)
                    issues.Add($"{orphanedResults} نتيجة تحليل بدون تحليل مريض مرتبط");

                var orphanedTests = await _context.PatientTests
                    .Where(pt => !_context.Patients.Any(p => p.PatientId == pt.PatientId) ||
                                !_context.TestTypes.Any(tt => tt.TestTypeId == pt.TestTypeId))
                    .CountAsync();
                
                if (orphanedTests > 0)
                    issues.Add($"{orphanedTests} تحليل مريض بدون مريض أو نوع تحليل مرتبط");

                // Check for duplicate patient numbers
                var duplicatePatients = await _context.Patients
                    .GroupBy(p => p.PatientNumber)
                    .Where(g => g.Count() > 1)
                    .CountAsync();
                
                if (duplicatePatients > 0)
                    issues.Add($"{duplicatePatients} رقم مريض مكرر");

                // Check for duplicate usernames
                var duplicateUsers = await _context.Users
                    .GroupBy(u => u.Username)
                    .Where(g => g.Count() > 1)
                    .CountAsync();
                
                if (duplicateUsers > 0)
                    issues.Add($"{duplicateUsers} اسم مستخدم مكرر");

                if (issues.Any())
                {
                    var issueText = string.Join("; ", issues);
                    await _loggingService.LogWarningAsync($"مشاكل في سلامة البيانات: {issueText}", "TestData");
                    return false;
                }

                await _loggingService.LogInfoAsync("تم التحقق من سلامة البيانات - لا توجد مشاكل", "TestData");
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في التحقق من سلامة البيانات", ex, "TestData");
                return false;
            }
        }

        public async Task<string> GetSampleDataSummaryAsync()
        {
            try
            {
                var summary = new StringBuilder();
                summary.AppendLine("ملخص البيانات التجريبية - OGRA LAB");
                summary.AppendLine($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                summary.AppendLine(new string('=', 50));
                summary.AppendLine();

                // Count records by creator
                var sampleUsers = await _context.Users.Where(u => u.CreatedBy == "TestDataService").CountAsync();
                var samplePatients = await _context.Patients.Where(p => p.CreatedBy == "TestDataService").CountAsync();
                var sampleTestTypes = await _context.TestTypes.Where(tt => tt.CreatedBy == "TestDataService").CountAsync();
                var sampleTestGroups = await _context.TestGroups.Where(tg => tg.CreatedBy == "TestDataService").CountAsync();
                var samplePatientTests = await _context.PatientTests.Where(pt => pt.CreatedBy == "TestDataService").CountAsync();
                var sampleTestResults = await _context.TestResults.Where(tr => tr.CreatedBy == "TestDataService").CountAsync();

                summary.AppendLine("البيانات التجريبية:");
                summary.AppendLine($"المستخدمون: {sampleUsers}");
                summary.AppendLine($"المرضى: {samplePatients}");
                summary.AppendLine($"أنواع التحاليل: {sampleTestTypes}");
                summary.AppendLine($"مجموعات التحاليل: {sampleTestGroups}");
                summary.AppendLine($"تحاليل المرضى: {samplePatientTests}");
                summary.AppendLine($"نتائج التحاليل: {sampleTestResults}");
                summary.AppendLine();

                // Total counts
                var totalUsers = await _context.Users.CountAsync();
                var totalPatients = await _context.Patients.CountAsync();
                var totalTestTypes = await _context.TestTypes.CountAsync();
                var totalTestGroups = await _context.TestGroups.CountAsync();
                var totalPatientTests = await _context.PatientTests.CountAsync();
                var totalTestResults = await _context.TestResults.CountAsync();

                summary.AppendLine("إجمالي البيانات:");
                summary.AppendLine($"المستخدمون: {totalUsers}");
                summary.AppendLine($"المرضى: {totalPatients}");
                summary.AppendLine($"أنواع التحاليل: {totalTestTypes}");
                summary.AppendLine($"مجموعات التحاليل: {totalTestGroups}");
                summary.AppendLine($"تحاليل المرضى: {totalPatientTests}");
                summary.AppendLine($"نتائج التحاليل: {totalTestResults}");

                return summary.ToString();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء ملخص البيانات التجريبية", ex, "TestData");
                return "خطأ في إنشاء ملخص البيانات التجريبية";
            }
        }

        #endregion
    }
}
