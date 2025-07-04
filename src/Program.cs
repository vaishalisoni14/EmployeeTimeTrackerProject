using EmployeeTimeTrackerProject;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;

namespace EmployeeTimeTracker.src
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            var timeEntries = await FetchTimeEntries(apiUrl);

            var employeeHours = timeEntries
                .Where(e => e.DeletedOn == null && !string.IsNullOrEmpty(e.EmployeeName) &&
                            e.StarTimeUtc != null && e.EndTimeUtc != null && e.EndTimeUtc >= e.StarTimeUtc)
                .GroupBy(e => e.EmployeeName)
                .Select(g => new EmployeeHours
                {
                    Name = g.Key,
                    TotalHours = g.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours)
                })
                .OrderByDescending(e => e.TotalHours)
                .ToList();

            GenerateHtmlTable(employeeHours, $"employees.html");
            GeneratePieChart(employeeHours, $"piechart.png");
        }

        static async Task<List<TimeEntry>> FetchTimeEntries(string apiUrl)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(apiUrl);
                var entries = JsonConvert.DeserializeObject<List<TimeEntry>>(response);
                return entries?.Where(e => e != null).ToList() ?? new List<TimeEntry>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to fetch time entries: {ex.Message}");
                return new List<TimeEntry>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize time entries: {ex.Message}");
                return new List<TimeEntry>();
            }
        }

        static void GenerateHtmlTable(List<EmployeeHours> employeeHours, string fileName)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><style>");
            html.AppendLine("table { border-collapse: collapse; width: 60%; margin: 20px auto; box-shadow: 0 2px 5px rgba(0,0,0,0.1); table-layout: fixed; }");
            html.AppendLine("th, td { border: 1px solid black; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #A9A9A9; }");
            html.AppendLine("th:nth-child(1), td:nth-child(1) { width: 30%; }");
            html.AppendLine("th:nth-child(2), td:nth-child(2) { width: 30%; }");
            html.AppendLine(".low-hours { background-color: #FF4040; }");
            html.AppendLine("</style></head><body>");
            html.AppendLine("<h2 style='text-align: center;'>Employee Hours Worked</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Name</th><th>Total Hours Worked</th></tr>");

            foreach (var emp in employeeHours)
            {
                var rowClass = emp.TotalHours < 100 ? " class='low-hours'" : "";
                html.AppendLine($"<tr{rowClass}><td>{emp.Name}</td><td>{emp.TotalHours:F2}</td></tr>");
            }

            html.AppendLine("</table></body></html>");
            File.WriteAllText(fileName, html.ToString());
            Console.WriteLine($"HTML table generated: {fileName}");
        }

        static void GeneratePieChart(List<EmployeeHours> employeeHours, string fileName)
        {
            try
            {
                int width = 800, height = 600;
                using var bitmap = new Bitmap(width, height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.White);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                float totalHours = (float)employeeHours.Sum(e => e.TotalHours);
                if (totalHours == 0)
                {
                    Console.WriteLine("No hours to visualize. Pie chart not generated.");
                    return;
                }

                float startAngle = 0;

                Color[] colors = GenerateColors(employeeHours.Count);

                for (int i = 0; i < employeeHours.Count; i++)
                {
                    float sweepAngle = (float)(employeeHours[i].TotalHours / totalHours * 360);
                    if (sweepAngle > 0)
                    {
                        graphics.FillPie(new SolidBrush(colors[i]), 100, 90, 300, 300, startAngle, sweepAngle);
                        startAngle += sweepAngle;
                    }
                }


                using (var headingBrush = new SolidBrush(Color.LightGray))
                using (var headingFont = new Font("Arial", 16, FontStyle.Bold))
                {
                    graphics.FillRectangle(headingBrush, 0, 0, width, 40);
                    string headingText = "Employee Hours Distribution";
                    SizeF textSize = graphics.MeasureString(headingText, headingFont);
                    float x = (width - textSize.Width) / 2;
                    graphics.DrawString(headingText, headingFont, Brushes.Black, x, 10);
                }

                int legendX = 510, legendY = 130, legendSpacing = 20;
                int maxLegendEntries = (height - legendY - 20) / legendSpacing;
                for (int i = 0; i < Math.Min(employeeHours.Count, maxLegendEntries); i++)
                {
                    graphics.FillRectangle(new SolidBrush(colors[i]), legendX, legendY + i * legendSpacing, 20, 10);
                    string legendText = $"{employeeHours[i].Name}: {(employeeHours[i].TotalHours / totalHours):P2}";
                    graphics.DrawString(legendText, new Font("Arial", 10), Brushes.Black, legendX + 25, legendY + i * legendSpacing);
                }
                if (employeeHours.Count > maxLegendEntries)
                {
                    graphics.DrawString($"... and {employeeHours.Count - maxLegendEntries} more", new Font("Arial", 10, FontStyle.Italic),
                        Brushes.Black, legendX, legendY + maxLegendEntries * legendSpacing);
                }

                bitmap.Save(fileName, ImageFormat.Png);
                Console.WriteLine($"Pie chart generated: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate pie chart: {ex.Message}");
            }
        }

        static Color[] GenerateColors(int count)
        {
            var colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                float hue = i * 360f / count;
                int r = (int)(Math.Sin(hue * Math.PI / 180) * 127 + 128);
                int g = (int)(Math.Sin((hue + 120) * Math.PI / 180) * 127 + 128);
                int b = (int)(Math.Sin((hue + 240) * Math.PI / 180) * 127 + 128);
                colors[i] = Color.FromArgb(r, g, b);
            }
            return colors;
        }
    }
}
