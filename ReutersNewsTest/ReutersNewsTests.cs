using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Configuration;
using NUnit.Framework;

namespace ReutersNewsTest
{
	[TestFixture]
	public class ReutersNewsTests
	{
		#region Fields & Properties
		public static string Result { get; set; }
		public static string Subject { get; set; }
		public static bool CommonPassed { get; set; }
		public TestContext TestContext { get; set; }
		private static IWebDriver _driver;
		#endregion
		
		#region Initializing
		[TestFixtureSetUp]
		public static void Init()
		{
			_driver = new FirefoxDriver();
			CommonPassed = true;
			Result = string.Empty;
			Subject = "Test Results of MagicNews/NewsMax stability";
		}
		#endregion

		#region Tests
		[Test]
		public void MagicNews()
		{
			_driver.Navigate().GoToUrl(ConfigurationManager.AppSettings["MagicNewsUrl"]);
			WaitForElement(_driver, By.CssSelector(".accordionitem.ng-scope"));
			_driver.FindElement(By.CssSelector(".accordionitem.ng-scope")).Click();
			_driver.FindElement(By.Id("uploadBtn")).SendKeys(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())) + "\\" + ConfigurationManager.AppSettings["FileToUpload"]);
			_driver.FindElement(By.CssSelector(".flex-item.submitbutton")).Click();

			WaitForElement(_driver, By.XPath("//*[contains(@class, 'result-item repeated-item')]"));
			var result = _driver.FindElement(By.XPath("//input[@name='range0']"));
			var value = result.GetAttribute("value");

			Assert.IsTrue(value.Equals("182 results"));
		}

		[Test]
		public void NewsMax()
		{
			_driver.Navigate().GoToUrl(ConfigurationManager.AppSettings["NewsMaxUrl"]);
			WaitForElement(_driver, By.CssSelector(".accordionitem.V2.ng-scope"));
			_driver.FindElement(By.CssSelector(".accordionitem.V2.ng-scope")).Click();
			_driver.FindElement(By.Id("uploadBtn")).SendKeys(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())) + "\\" + ConfigurationManager.AppSettings["FileToUpload"]);
			_driver.FindElement(By.CssSelector(".flex-item.submitbutton")).Click();

			WaitForElement(_driver, By.XPath("//*[contains(@class, 'result-item repeated-item')]"));
			var result = _driver.FindElement(By.XPath("//input[@name='range0']"));
			var value = result.GetAttribute("value");

			Assert.IsTrue(value.Equals("182 results"));
		}
		#endregion

		#region Cleanup
		[TearDown]
		public void CollectReport()
		{
			if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
			{
				CommonPassed = false;
				Result += string.Format("{0} test was failed. Possibly there is a problem. Please check it ASAP!!!\n", TestContext.Test.Name);
			}
			else
			{
				Result += string.Format("{0} test passed. Looks like everything works fine.\n", TestContext.Test.Name);
			}
		}
		
		[TestFixtureTearDown]
		public static void SendReport()
		{
			_driver.Quit();
			Subject = (CommonPassed ? "PASSED: " : "FAILED: ") + Subject;
			SendMail();
		}
		#endregion

		#region Methods
		public static void SendMail()
		{
			SmtpClient client = new SmtpClient
			{
				Port = 587,
				Host = "smtp.gmail.com",
				EnableSsl = true,
				Timeout = 10000,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["FromUsername"], ConfigurationManager.AppSettings["FromPassword"])
			};

			MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["FromUsername"], "me@me.com", Subject, Result)
			{
				BodyEncoding = Encoding.UTF8,
				DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
			};
			foreach (var mail in ConfigurationManager.AppSettings["ToMails"].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
			{
				mm.To.Add(mail);
			}

			client.Send(mm);
		}

		public static void WaitForElement(IWebDriver driver, By by, int seconds = 30)
		{
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
			wait.Until(d => d.FindElement(by).Displayed);
		}
		#endregion
	}
}
