using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using OGRALAB.Models;
using System.Globalization;

namespace OGRALAB.Views
{
    public partial class ReportPreviewWindow : Window
    {
        private Patient _patient;
        private IEnumerable<PatientTest> _tests;
        private bool _autoPrint;
        private double _currentZoom = 100;

        public ReportPreviewWindow(Patient patient, IEnumerable<PatientTest> tests, bool autoPrint = false)
        {
            InitializeComponent();
            
            _patient = patient;
            _tests = tests;
            _autoPrint = autoPrint;
            
            GenerateReport();
            
            if (_autoPrint)
            {
                HeaderTitle.Text = "طباعة تقرير نتائج المريض";
                HeaderSubtitle.Text = "جاري تحضير التقرير للطباعة";
                PrintButton.Content = "🖨️ طباعة الآن";
            }
        }

        private void GenerateReport()
        {
            try
            {
                var reportDocument = CreateReportDocument();
                ReportViewer.Document = reportDocument;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreateReportDocument()
        {
            var document = new FlowDocument();
            document.PagePadding = new Thickness(40);
            document.ColumnWidth = double.PositiveInfinity;
            document.FontFamily = new FontFamily("Cairo, Segoe UI, Arial");
            document.FontSize = 12;
            document.Background = Brushes.White;

            // Header Section
            AddReportHeader(document);
            
            // Patient Information Section
            AddPatientInformation(document);
            
            // Tests Results Section
            AddTestsResults(document);
            
            // Footer Section
            AddReportFooter(document);

            return document;
        }

        private void AddReportHeader(FlowDocument document)
        {
            // Lab Header
            var headerTable = new Table();
            headerTable.CellSpacing = 0;
            headerTable.BorderBrush = Brushes.Black;
            headerTable.BorderThickness = new Thickness(1);

            // Define columns
            headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var headerRowGroup = new TableRowGroup();
            var headerRow = new TableRow();

            // Lab Logo and Name (Left side)
            var logoCell = new TableCell();
            logoCell.BorderBrush = Brushes.Black;
            logoCell.BorderThickness = new Thickness(0, 0, 1, 0);
            logoCell.Padding = new Thickness(10);
            logoCell.VerticalAlignment = VerticalAlignment.Center;
            
            var logoBlock = new Paragraph();
            logoBlock.TextAlignment = TextAlignment.Center;
            logoBlock.Inlines.Add(new Run("🧪 OGRA LAB") 
            { 
                FontSize = 24, 
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138))
            });
            logoBlock.Inlines.Add(new LineBreak());
            logoBlock.Inlines.Add(new Run("مختبر أوجرا للتحاليل الطبية") 
            { 
                FontSize = 16, 
                FontWeight = FontWeights.SemiBold 
            });
            logoBlock.Inlines.Add(new LineBreak());
            logoBlock.Inlines.Add(new Run("Medical Laboratory Services") 
            { 
                FontSize = 12, 
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });
            
            logoCell.Blocks.Add(logoBlock);

            // Lab Info (Right side)
            var infoCell = new TableCell();
            infoCell.Padding = new Thickness(10);
            infoCell.VerticalAlignment = VerticalAlignment.Center;
            
            var infoBlock = new Paragraph();
            infoBlock.TextAlignment = TextAlignment.Left;
            infoBlock.Inlines.Add(new Run("📍 Address: Medical District, Healthcare City") 
            { 
                FontSize = 10 
            });
            infoBlock.Inlines.Add(new LineBreak());
            infoBlock.Inlines.Add(new Run("📞 Phone: +966 11 234 5678") 
            { 
                FontSize = 10 
            });
            infoBlock.Inlines.Add(new LineBreak());
            infoBlock.Inlines.Add(new Run("📧 Email: info@ogralab.com") 
            { 
                FontSize = 10 
            });
            infoBlock.Inlines.Add(new LineBreak());
            infoBlock.Inlines.Add(new Run("🌐 Website: www.ogralab.com") 
            { 
                FontSize = 10 
            });
            
            infoCell.Blocks.Add(infoBlock);

            headerRow.Cells.Add(logoCell);
            headerRow.Cells.Add(infoCell);
            headerRowGroup.Rows.Add(headerRow);
            headerTable.RowGroups.Add(headerRowGroup);

            document.Blocks.Add(headerTable);
            document.Blocks.Add(new Paragraph(new LineBreak()));
        }

        private void AddPatientInformation(FlowDocument document)
        {
            // Patient Information Title
            var titleParagraph = new Paragraph();
            titleParagraph.TextAlignment = TextAlignment.Center;
            titleParagraph.Margin = new Thickness(0, 10, 0, 15);
            titleParagraph.Inlines.Add(new Run("تقرير نتائج التحاليل الطبية") 
            { 
                FontSize = 18, 
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138))
            });
            titleParagraph.Inlines.Add(new LineBreak());
            titleParagraph.Inlines.Add(new Run("MEDICAL TEST RESULTS REPORT") 
            { 
                FontSize = 14, 
                FontWeight = FontWeights.SemiBold,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });
            
            document.Blocks.Add(titleParagraph);

            // Patient Info Table
            var patientTable = new Table();
            patientTable.CellSpacing = 2;
            patientTable.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            patientTable.BorderBrush = Brushes.LightGray;
            patientTable.BorderThickness = new Thickness(1);

            // Define columns for patient info
            patientTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            patientTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var patientRowGroup = new TableRowGroup();

            // Row 1: Patient Name and Age
            var row1 = new TableRow();
            
            var nameCell = new TableCell();
            nameCell.Padding = new Thickness(8);
            nameCell.BorderBrush = Brushes.LightGray;
            nameCell.BorderThickness = new Thickness(0, 0, 1, 1);
            var nameParagraph = new Paragraph();
            nameParagraph.Inlines.Add(new Run("اسم المريض: ") { FontWeight = FontWeights.Bold });
            
            // Add title if exists
            var fullName = _patient.FullName;
            if (!string.IsNullOrEmpty(_patient.Title))
            {
                fullName = $"{_patient.Title} {fullName}";
            }
            
            nameParagraph.Inlines.Add(new Run(fullName));
            nameCell.Blocks.Add(nameParagraph);

            var ageCell = new TableCell();
            ageCell.Padding = new Thickness(8);
            ageCell.BorderBrush = Brushes.LightGray;
            ageCell.BorderThickness = new Thickness(0, 0, 0, 1);
            var ageParagraph = new Paragraph();
            ageParagraph.Inlines.Add(new Run("العمر والجنس: ") { FontWeight = FontWeights.Bold });
            
            var ageText = $"{_patient.Age} {_patient.AgeUnit}";
            var genderText = _patient.Gender switch
            {
                "Male" => "Male",
                "Female" => "Female", 
                _ => "Unknown"
            };
            
            ageParagraph.Inlines.Add(new Run($"{ageText} - {genderText}"));
            ageCell.Blocks.Add(ageParagraph);

            row1.Cells.Add(nameCell);
            row1.Cells.Add(ageCell);
            patientRowGroup.Rows.Add(row1);

            // Row 2: Doctor and Registration Date
            var row2 = new TableRow();
            
            var doctorCell = new TableCell();
            doctorCell.Padding = new Thickness(8);
            doctorCell.BorderBrush = Brushes.LightGray;
            doctorCell.BorderThickness = new Thickness(0, 0, 1, 1);
            var doctorParagraph = new Paragraph();
            doctorParagraph.Inlines.Add(new Run("الطبيب المحول: ") { FontWeight = FontWeights.Bold });
            
            var doctorName = "";
            if (!string.IsNullOrEmpty(_patient.DoctorTitle) || !string.IsNullOrEmpty(_patient.DoctorName))
            {
                doctorName = $"أ.د/ {_patient.DoctorTitle} {_patient.DoctorName}".Trim();
            }
            else
            {
                doctorName = "غير محدد";
            }
            
            doctorParagraph.Inlines.Add(new Run(doctorName));
            doctorCell.Blocks.Add(doctorParagraph);

            var regDateCell = new TableCell();
            regDateCell.Padding = new Thickness(8);
            regDateCell.BorderBrush = Brushes.LightGray;
            regDateCell.BorderThickness = new Thickness(0, 0, 0, 1);
            var regDateParagraph = new Paragraph();
            regDateParagraph.Inlines.Add(new Run("Registration Date: ") { FontWeight = FontWeights.Bold });
            regDateParagraph.Inlines.Add(new Run(_patient.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
            regDateCell.Blocks.Add(regDateParagraph);

            row2.Cells.Add(doctorCell);
            row2.Cells.Add(regDateCell);
            patientRowGroup.Rows.Add(row2);

            // Row 3: Patient ID and Print Date
            var row3 = new TableRow();
            
            var patientIdCell = new TableCell();
            patientIdCell.Padding = new Thickness(8);
            patientIdCell.BorderBrush = Brushes.LightGray;
            patientIdCell.BorderThickness = new Thickness(0, 0, 1, 0);
            var patientIdParagraph = new Paragraph();
            patientIdParagraph.Inlines.Add(new Run("Patient ID: ") { FontWeight = FontWeights.Bold });
            patientIdParagraph.Inlines.Add(new Run(_patient.PatientNumber ?? "N/A"));
            patientIdCell.Blocks.Add(patientIdParagraph);

            var printDateCell = new TableCell();
            printDateCell.Padding = new Thickness(8);
            var printDateParagraph = new Paragraph();
            printDateParagraph.Inlines.Add(new Run("Print Date: ") { FontWeight = FontWeights.Bold });
            printDateParagraph.Inlines.Add(new Run(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)));
            printDateCell.Blocks.Add(printDateParagraph);

            row3.Cells.Add(patientIdCell);
            row3.Cells.Add(printDateCell);
            patientRowGroup.Rows.Add(row3);

            patientTable.RowGroups.Add(patientRowGroup);
            document.Blocks.Add(patientTable);
            document.Blocks.Add(new Paragraph(new LineBreak()));
        }

        private void AddTestsResults(FlowDocument document)
        {
            // Tests Results Title
            var resultsTitle = new Paragraph();
            resultsTitle.TextAlignment = TextAlignment.Center;
            resultsTitle.Margin = new Thickness(0, 20, 0, 15);
            resultsTitle.Inlines.Add(new Run("نتائج التحاليل") 
            { 
                FontSize = 16, 
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 138))
            });
            resultsTitle.Inlines.Add(new LineBreak());
            resultsTitle.Inlines.Add(new Run("TEST RESULTS") 
            { 
                FontSize = 12, 
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray
            });
            
            document.Blocks.Add(resultsTitle);

            // Tests Results Table
            var resultsTable = new Table();
            resultsTable.CellSpacing = 1;
            resultsTable.BorderBrush = Brushes.Black;
            resultsTable.BorderThickness = new Thickness(1);

            // Define columns: Test Name, Value, Unit, Alert, Normal Range
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // Test Name
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // Value
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(0.8, GridUnitType.Star) }); // Unit
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(0.5, GridUnitType.Star) }); // Alert
            resultsTable.Columns.Add(new TableColumn { Width = new GridLength(1.5, GridUnitType.Star) }); // Normal Range

            // Header Row
            var headerRowGroup = new TableRowGroup();
            headerRowGroup.Background = new SolidColorBrush(Color.FromRgb(30, 58, 138));
            
            var headerRow = new TableRow();
            
            AddTableHeaderCell(headerRow, "اسم التحليل");
            AddTableHeaderCell(headerRow, "النتيجة");
            AddTableHeaderCell(headerRow, "الوحدة");
            AddTableHeaderCell(headerRow, "تنبيه");
            AddTableHeaderCell(headerRow, "المعدل الطبيعي");
            
            headerRowGroup.Rows.Add(headerRow);
            resultsTable.RowGroups.Add(headerRowGroup);

            // Data Rows
            var dataRowGroup = new TableRowGroup();
            
            foreach (var test in _tests.Where(t => t.TestResults != null && t.TestResults.Any()))
            {
                foreach (var result in test.TestResults)
                {
                    var dataRow = new TableRow();
                    
                    // Alternate row colors
                    if (dataRowGroup.Rows.Count % 2 == 0)
                    {
                        dataRow.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                    }

                    // Test Name
                    AddTableDataCell(dataRow, test.TestType?.TestName ?? "Unknown Test");
                    
                    // Result Value
                    AddTableDataCell(dataRow, result.Value ?? "N/A");
                    
                    // Unit
                    AddTableDataCell(dataRow, test.TestType?.Unit ?? "");
                    
                    // Alert (*)
                    var alertText = result.IsNormal == false ? "*" : "";
                    var alertCell = new TableCell();
                    alertCell.Padding = new Thickness(5);
                    alertCell.BorderBrush = Brushes.LightGray;
                    alertCell.BorderThickness = new Thickness(1);
                    alertCell.TextAlignment = TextAlignment.Center;
                    
                    var alertParagraph = new Paragraph();
                    if (!string.IsNullOrEmpty(alertText))
                    {
                        alertParagraph.Inlines.Add(new Run(alertText) 
                        { 
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Red,
                            FontSize = 14
                        });
                    }
                    alertCell.Blocks.Add(alertParagraph);
                    dataRow.Cells.Add(alertCell);
                    
                    // Normal Range
                    AddTableDataCell(dataRow, test.TestType?.NormalRange ?? "");

                    dataRowGroup.Rows.Add(dataRow);
                }
            }

            resultsTable.RowGroups.Add(dataRowGroup);
            document.Blocks.Add(resultsTable);

            // Legend for abnormal values
            var legendParagraph = new Paragraph();
            legendParagraph.Margin = new Thickness(0, 10, 0, 0);
            legendParagraph.FontSize = 10;
            legendParagraph.Inlines.Add(new Run("* ") 
            { 
                FontWeight = FontWeights.Bold, 
                Foreground = Brushes.Red 
            });
            legendParagraph.Inlines.Add(new Run("قيم خارج النطاق الطبيعي / Values outside normal range"));
            
            document.Blocks.Add(legendParagraph);
        }

        private void AddTableHeaderCell(TableRow row, string text)
        {
            var cell = new TableCell();
            cell.Padding = new Thickness(8);
            cell.BorderBrush = Brushes.White;
            cell.BorderThickness = new Thickness(1);
            cell.TextAlignment = TextAlignment.Center;
            
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(text) 
            { 
                FontWeight = FontWeights.Bold, 
                Foreground = Brushes.White,
                FontSize = 12
            });
            
            cell.Blocks.Add(paragraph);
            row.Cells.Add(cell);
        }

        private void AddTableDataCell(TableRow row, string text)
        {
            var cell = new TableCell();
            cell.Padding = new Thickness(5);
            cell.BorderBrush = Brushes.LightGray;
            cell.BorderThickness = new Thickness(1);
            cell.TextAlignment = TextAlignment.Center;
            
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(text) { FontSize = 11 });
            
            cell.Blocks.Add(paragraph);
            row.Cells.Add(cell);
        }

        private void AddReportFooter(FlowDocument document)
        {
            document.Blocks.Add(new Paragraph(new LineBreak()));
            
            // Footer with signature area
            var footerParagraph = new Paragraph();
            footerParagraph.Margin = new Thickness(0, 30, 0, 0);
            footerParagraph.FontSize = 10;
            footerParagraph.TextAlignment = TextAlignment.Justify;
            
            footerParagraph.Inlines.Add(new Run("ملاحظات: ") { FontWeight = FontWeights.Bold });
            footerParagraph.Inlines.Add(new Run("هذا التقرير تم إنشاؤه إلكترونياً بواسطة نظام OGRA LAB لإدارة المختبرات الطبية."));
            footerParagraph.Inlines.Add(new LineBreak());
            footerParagraph.Inlines.Add(new Run("جميع النتائج تم مراجعتها من قبل أخصائي المختبر المعتمد."));
            footerParagraph.Inlines.Add(new LineBreak());
            footerParagraph.Inlines.Add(new LineBreak());
            
            footerParagraph.Inlines.Add(new Run("التوقيع: ____________________          التاريخ: ") { FontWeight = FontWeights.Bold });
            footerParagraph.Inlines.Add(new Run(DateTime.Now.ToString("dd/MM/yyyy")));
            
            document.Blocks.Add(footerParagraph);
            
            // Final footer
            var finalFooter = new Paragraph();
            finalFooter.Margin = new Thickness(0, 20, 0, 0);
            finalFooter.FontSize = 8;
            finalFooter.TextAlignment = TextAlignment.Center;
            finalFooter.Foreground = Brushes.Gray;
            finalFooter.Inlines.Add(new Run("OGRA LAB - Medical Laboratory Services | www.ogralab.com | Generated on "));
            finalFooter.Inlines.Add(new Run(DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
            
            document.Blocks.Add(finalFooter);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                
                if (printDialog.ShowDialog() == true)
                {
                    // Get the document to print
                    var document = ReportViewer.Document;
                    
                    // Print the document
                    printDialog.PrintDocument(
                        ((IDocumentPaginatorSource)document).DocumentPaginator,
                        $"تقرير نتائج المريض - {_patient.FullName}");
                    
                    MessageBox.Show("تم إرسال التقرير للطابعة بنجاح", "نجح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في طباعة التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsPdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"تقرير_نتائج_{_patient.FullName}_{DateTime.Now:yyyyMMdd}.pdf";
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    // For now, save as XPS (PDF requires additional libraries)
                    var xpsDialog = new SaveFileDialog();
                    xpsDialog.Filter = "XPS Files (*.xps)|*.xps";
                    xpsDialog.FileName = Path.ChangeExtension(saveFileDialog.FileName, ".xps");
                    
                    if (xpsDialog.ShowDialog() == true)
                    {
                        SaveAsXps(xpsDialog.FileName);
                        MessageBox.Show("تم حفظ التقرير بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsXps(string fileName)
        {
            var document = ReportViewer.Document;
            
            using (var xpsDocument = new XpsDocument(fileName, FileAccess.Write))
            {
                var xpsDocumentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                xpsDocumentWriter.Write(((IDocumentPaginatorSource)document).DocumentPaginator);
            }
        }

        private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Copy document content to clipboard as text
                var textRange = new TextRange(
                    ReportViewer.Document.ContentStart,
                    ReportViewer.Document.ContentEnd);
                
                Clipboard.SetText(textRange.Text);
                
                MessageBox.Show("تم نسخ محتوى التقرير للحافظة", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في نسخ التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom += 10;
            if (_currentZoom > 200) _currentZoom = 200;
            ReportViewer.Zoom = _currentZoom;
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom -= 10;
            if (_currentZoom < 50) _currentZoom = 50;
            ReportViewer.Zoom = _currentZoom;
        }
    }
}
