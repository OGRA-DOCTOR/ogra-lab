# سجل التغييرات (Changelog)

جميع التغييرات المهمة في هذا المشروع سيتم توثيقها في هذا الملف.

التنسيق مبني على [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)،
وهذا المشروع يتبع [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [غير منشور]

### مخطط للإضافة
- واجهة تسجيل الدخول المتقدمة
- واجهات إدارة المرضى
- واجهات إدارة التحاليل
- نظام الطباعة
- تقارير مفصلة
- نظام النسخ الاحتياطي
- إعدادات النظام

## [1.0.0] - 2024-05-27

### أضيف
- إعداد المشروع الأولي مع .NET 8.0 و WPF
- نمط MVVM الأساسي
- قاعدة بيانات SQLite مع Entity Framework Core
- نماذج البيانات الكاملة:
  - User (المستخدمين)
  - Patient (المرضى)
  - TestType (أنواع التحاليل)
  - TestGroup (مجموعات التحاليل)
  - PatientTest (طلبات التحاليل)
  - TestResult (نتائج التحاليل)
  - LoginLog (سجل الدخول)
- طبقة الخدمات الشاملة:
  - IUserService و UserService
  - IPatientService و PatientService
  - ITestService و TestService
  - IAuthenticationService و AuthenticationService
  - IReportService و ReportService
- أدوات مساعدة:
  - DatabaseHelper (مساعد قاعدة البيانات)
  - PasswordHelper (مساعد كلمات المرور)
  - ValidationHelper (مساعد التحقق)
- تشفير كلمات المرور باستخدام BCrypt
- الواجهة الرئيسية مع لوحة التحكم
- دعم اللغة العربية (RTL)
- نظام حقن التبعيات
- إعدادات التطبيق الشاملة
- مستخدم مدير افتراضي (admin/admin123)
- ملفات التوثيق والترخيص

### التفاصيل التقنية
- إطار العمل: .NET 8.0
- واجهة المستخدم: WPF
- قاعدة البيانات: SQLite
- ORM: Entity Framework Core 8.0
- تشفير كلمات المرور: BCrypt.Net-Next
- نمط التصميم: MVVM
- حقن التبعيات: Microsoft.Extensions.DependencyInjection

### البيانات الأولية
- مستخدم مدير افتراضي
- مجموعات تحاليل أساسية (كيمياء، هرمونات، دم)
- أنواع تحاليل أساسية (سكر، كوليسترول، TSH، CBC)

### الأمان
- تشفير كلمات المرور
- تسجيل محاولات تسجيل الدخول
- نظام الأدوار والصلاحيات
- حماية من حقن SQL

### الوثائق
- README.md شامل بالعربية والإنجليزية
- تعليقات كاملة في الكود
- توثيق هيكل قاعدة البيانات
- دليل التثبيت والإعداد

### الملفات الإضافية
- .gitignore محدث
- App.config مع إعدادات شاملة
- LICENSE (MIT)
- CHANGELOG.md
