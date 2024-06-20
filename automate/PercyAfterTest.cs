using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Newtonsoft.Json;
using NUnit.Framework;
using PercyIO.Playwright;

namespace PercyOnAutomate
{
    [TestFixture]
    [Category("automate-percy-after-test")]
    public class PercyAfterTest
    {
        private IBrowser browser;
        private IBrowserContext context;
        private IPage page;
        private readonly string browserstackUsername = Environment.GetEnvironmentVariable("BROWSERSTACK_USERNAME");
        private readonly string browserstackAccessKey = Environment.GetEnvironmentVariable("BROWSERSTACK_ACCESS_KEY");
        private readonly string browserstackUrl = "wss://cdp.browserstack.com/playwright?caps=";

        public PercyAfterTest() : base() { }

        [SetUp]
        public async Task Init()
        {
            try
            {
                var browserstackOptions = new Dictionary<string, object>
                {
                    { "browser", "chrome" },
                    { "browser_version", "latest" },
                    { "os", "osx" },
                    { "os_version", "ventura" },
                    { "name", "Percy Playwright Dotnet Test" },
                    { "build", "percy-playwright-dotnet-test" },
                    { "browserstack.username", browserstackUsername },
                    { "browserstack.accessKey", browserstackAccessKey }
                };

                string capsJson = JsonConvert.SerializeObject(browserstackOptions);
                string cdpUrl = browserstackUrl + Uri.EscapeDataString(capsJson);

                var playwright = await Playwright.CreateAsync();
                browser = await playwright.Chromium.ConnectAsync(cdpUrl);
                page = await browser.NewPageAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Test]
        public async Task SearchBstackDemo()
        {
            try
            {
                // Navigate to the page
                await page.SetViewportSizeAsync(1280, 1024);
                await page.GotoAsync("https://bstackdemo.com/");

                // Check the title
                var pageTitle = await page.TitleAsync();
                Assert.IsTrue(pageTitle.Contains("StackDemo"));

                // Click on the Samsung products
                await page.ClickAsync("//*[@id='__next']/div/div/main/div[1]/div[2]/label/span");

                // Percy Screenshot 1
                Percy.Screenshot(page, "screenshot_1");

                // Get the text of the current product
                var productOnPageText = await page.TextContentAsync("//*[@id=\"10\"]/p");

                // Click on 'Add to cart' button
                await page.ClickAsync("//*[@id=\"10\"]/div[4]");

                // Check if the Cart pane is visible
                await page.WaitForSelectorAsync("//*[@class=\"float-cart__content\"]");

                // Get the text of the product in the cart
                var productOnCartText = await page.TextContentAsync("//*[@id='__next']/div/div/div[2]/div[2]/div[2]/div/div[3]/p[1]");

                // Percy Screenshot 2
                // with options
                var options = new Dictionary<string, object>
                {
                  { "test_case", "should add product to cart" },
                };
                Percy.Screenshot(page, "screenshot_2", options);

                // Assert that the product text matches
                Assert.AreEqual(productOnCartText, productOnPageText);
                if (productOnCartText == productOnPageText)
                {
                    await MarkTestStatus("passed", "Title matched", page);
                }
                else
                {
                    await MarkTestStatus("failed", "Tile did not matched", page);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TearDown]
        public async Task Cleanup()
        {
            try
            {
                if (page != null)
                {
                    await page.CloseAsync();
                }

                if (context != null)
                {
                    await context.CloseAsync();
                }

                if (browser != null)
                {
                    await browser.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task MarkTestStatus(string status, string reason, IPage page) 
        {
            await page.EvaluateAsync("_ => {}", "browserstack_executor: {\"action\": \"setSessionStatus\", \"arguments\": {\"status\":\"" + status + "\", \"reason\": \"" + reason + "\"}}");
        }    
    }
}
