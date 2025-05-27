# دليل المطور - OGRA LAB

## نظرة عامة سريعة
هذا دليل سريع للمطورين للبدء في العمل على مشروع OGRA LAB.

## البدء السريع

### 1. المتطلبات
```bash
# تأكد من تثبيت .NET 8.0 SDK
dotnet --version  # يجب أن يكون 8.0 أو أحدث
```

### 2. استنساخ وبناء المشروع
```bash
git clone https://github.com/OGRA-DOCTOR/ogra-lab.git
cd ogra-lab
dotnet restore
dotnet build
```

### 3. تشغيل المشروع
```bash
dotnet run
```

### 4. بيانات الدخول الافتراضية
- **اسم المستخدم**: admin
- **كلمة المرور**: admin123

## هيكل المشروع

```
OGRALAB/
├── Models/                 # نماذج البيانات
│   ├── User.cs            # نموذج المستخدم
│   ├── Patient.cs         # نموذج المريض
│   ├── TestType.cs        # نموذج نوع التحليل
│   ├── TestGroup.cs       # نموذج مجموعة التحاليل
│   ├── PatientTest.cs     # نموذج طلب التحليل
│   ├── TestResult.cs      # نموذج نتيجة التحليل
│   └── LoginLog.cs        # نموذج سجل الدخول
├── Services/              # طبقة الخدمات
│   ├── IUserService.cs    # واجهة خدمة المستخدمين
│   ├── UserService.cs     # خدمة المستخدمين
│   ├── IPatientService.cs # واجهة خدمة المرضى
│   ├── PatientService.cs  # خدمة المرضى
│   ├── ITestService.cs    # واجهة خدمة التحاليل
│   ├── TestService.cs     # خدمة التحاليل
│   ├── IAuthenticationService.cs # واجهة خدمة المصادقة
│   ├── AuthenticationService.cs  # خدمة المصادقة
│   ├── IReportService.cs  # واجهة خدمة التقارير
│   └── ReportService.cs   # خدمة التقارير
├── Views/                 # واجهات المستخدم
│   ├── MainWindow.xaml    # النافذة الرئيسية
│   └── MainWindow.xaml.cs # منطق النافذة الرئيسية
├── ViewModels/            # نماذج العرض (MVVM)
├── Data/                  # قاعدة البيانات
│   └── OgraLabDbContext.cs # سياق قاعدة البيانات
├── Helpers/               # أدوات مساعدة
│   ├── DatabaseHelper.cs  # مساعد قاعدة البيانات
│   ├── PasswordHelper.cs  # مساعد كلمات المرور
│   └── ValidationHelper.cs # مساعد التحقق
├── App.xaml              # إعدادات التطبيق
├── App.xaml.cs           # منطق بدء التطبيق
└── OGRALAB.csproj        # ملف المشروع
```

## قاعدة البيانات

### الجداول الرئيسية
1. **Users** - المستخدمين ومعلومات المصادقة
2. **Patients** - بيانات المرضى
3. **TestTypes** - أنواع التحاليل المتاحة
4. **TestGroups** - مجموعات التحاليل
5. **PatientTests** - طلبات التحاليل
6. **TestResults** - نتائج التحاليل
7. **LoginLogs** - سجل الدخول والخروج

### العلاقات
- User -> LoginLogs (1:M)
- Patient -> PatientTests (1:M)
- TestType -> PatientTests (1:M)
- TestType -> TestResults (1:M)
- TestGroup -> TestTypes (1:M)
- PatientTest -> TestResults (1:M)

## طبقة الخدمات

### خدمة المستخدمين (UserService)
```csharp
var userService = App.GetService<IUserService>();
var users = await userService.GetAllUsersAsync();
var user = await userService.GetUserByIdAsync(1);
```

### خدمة المرضى (PatientService)
```csharp
var patientService = App.GetService<IPatientService>();
var patients = await patientService.GetAllPatientsAsync();
var patient = await patientService.GetPatientByIdAsync(1);
```

### خدمة التحاليل (TestService)
```csharp
var testService = App.GetService<ITestService>();
var testTypes = await testService.GetAllTestTypesAsync();
var patientTests = await testService.GetPatientTestsByPatientIdAsync(1);
```

### خدمة المصادقة (AuthenticationService)
```csharp
var authService = App.GetService<IAuthenticationService>();
var user = await authService.AuthenticateAsync("admin", "admin123");
var isAuthenticated = authService.IsAuthenticated;
```

## أمثلة على الاستخدام

### إضافة مريض جديد
```csharp
var patientService = App.GetService<IPatientService>();
var patient = new Patient
{
    FullName = "أحمد محمد علي",
    NationalId = "1234567890",
    DateOfBirth = new DateTime(1990, 1, 1),
    Gender = "Male",
    PhoneNumber = "0501234567"
};

var createdPatient = await patientService.CreatePatientAsync(patient);
```

### طلب تحليل جديد
```csharp
var testService = App.GetService<ITestService>();
var patientTest = new PatientTest
{
    PatientId = 1,
    TestTypeId = 1,
    TotalAmount = 100.00m,
    PaidAmount = 100.00m,
    CreatedByUserId = 1
};

var createdTest = await testService.CreatePatientTestAsync(patientTest);
```

### إدخال نتيجة تحليل
```csharp
var testService = App.GetService<ITestService>();
var result = new TestResult
{
    PatientTestId = 1,
    TestTypeId = 1,
    ResultValue = "95",
    NumericValue = 95,
    Unit = "mg/dL",
    Status = "Normal",
    CreatedByUserId = 1
};

var createdResult = await testService.CreateTestResultAsync(result);
```

## التطوير التدريجي

### المرحلة التالية (Phase 2)
1. **واجهة تسجيل الدخول**
   - إنشاء LoginWindow.xaml
   - إنشاء LoginViewModel
   - ربط خدمة المصادقة

2. **واجهات إدارة المرضى**
   - إنشاء PatientListWindow.xaml
   - إنشاء PatientEditWindow.xaml
   - إنشاء PatientViewModel

3. **واجهات إدارة التحاليل**
   - إنشاء TestOrderWindow.xaml
   - إنشاء TestResultWindow.xaml
   - إنشاء TestViewModel

### نصائح للتطوير

#### استخدام نمط MVVM
```csharp
// في ViewModel
public class PatientViewModel : INotifyPropertyChanged
{
    private readonly IPatientService _patientService;
    
    public PatientViewModel(IPatientService patientService)
    {
        _patientService = patientService;
    }
    
    // Properties and Commands
}
```

#### حقن التبعيات
```csharp
// في App.xaml.cs
services.AddTransient<PatientViewModel>();
services.AddTransient<PatientListWindow>();
```

#### معالجة الأخطاء
```csharp
try
{
    var result = await _patientService.CreatePatientAsync(patient);
    // Success handling
}
catch (InvalidOperationException ex)
{
    MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

## اختبار المشروع

### اختبار الوحدة
```csharp
[Test]
public async Task CreatePatient_ValidData_ReturnsPatient()
{
    // Arrange
    var patient = new Patient { /* valid data */ };
    
    // Act
    var result = await _patientService.CreatePatientAsync(patient);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(patient.FullName, result.FullName);
}
```

## الأدوات المفيدة

### أدوات التحقق
```csharp
var isValidEmail = ValidationHelper.IsValidEmail("test@example.com");
var isValidPhone = ValidationHelper.IsValidPhoneNumber("0501234567");
var isValidNationalId = ValidationHelper.IsValidNationalId("1234567890");
```

### أدوات كلمات المرور
```csharp
var hash = PasswordHelper.HashPassword("mypassword");
var isValid = PasswordHelper.VerifyPassword("mypassword", hash);
var strength = PasswordHelper.CheckPasswordStrength("mypassword");
```

### أدوات قاعدة البيانات
```csharp
var dbPath = DatabaseHelper.GetDatabasePath();
var backupPath = DatabaseHelper.GetBackupPath();
var success = await DatabaseHelper.BackupDatabaseAsync(dbPath, backupPath);
```

## الدعم والمساعدة

### الأخطاء الشائعة
1. **خطأ في قاعدة البيانات**: تأكد من وجود مجلد Data
2. **خطأ في التشغيل**: تأكد من تثبيت .NET 8.0
3. **خطأ في الحزم**: قم بتشغيل `dotnet restore`

### تصحيح الأخطاء
```csharp
// تمكين سجل مفصل في App.xaml.cs
services.AddLogging(builder => builder.AddDebug().AddConsole());
```

### الحصول على المساعدة
- راجع README.md للتوثيق الكامل
- راجع CHANGELOG.md لآخر التحديثات
- أنشئ Issue في GitHub للمشاكل التقنية

---

**ملاحظة**: هذا المشروع في مرحلة التطوير النشط. تأكد من مراجعة آخر التحديثات في GitHub.
