using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TGH.Common.Utilities.Logging;

namespace TGH.Common.Utilities.DataLoader.Extensions
{
	public static class DataLoaderExtensions
	{
		#region File-Specific Constant(s)
		private const string HTTP_REQUEST_RETRY_FAILURE = "Could not execute GET request.  Exceeded the maximum number of retries.";
		private const string BASE64_CONVERSION_ERROR_TEMPLATE = "Error converting base64 to bitmap: {0}";
		private const string BITMAP_BASE64_CONVERSION_ERROR_TEMPLATE = "Error converting bitmap to base64: {0}";
		private const string BITMAP_BYTEARRAY_CONVERSION_ERROR_TEMPLATE = "Error converting bitmap to byte array: {0}";
		#endregion



		#region Data Retrieval Extension(s)
		public static async Task<HttpResponseMessage> GetAsyncWithAutoRetry(this HttpClient client, string requestURL, int retryCount = 5)
		{
			int initialRetries = retryCount;

			while (retryCount > 0 || retryCount == -1)
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(requestURL);
					response.EnsureSuccessStatusCode();
					return response;
				}
				catch (Exception)
				{
					retryCount--;
					/*if(retryCount > 0)
					{
						Logger.LogVerbose($"Retrying... (Attempt {initialRetries - retryCount + 1} of {initialRetries})");
					}*/
				}
			}

			throw new HttpRequestException(HTTP_REQUEST_RETRY_FAILURE);
		}
		#endregion



		#region Base64 Image Extension(s)
		public static Bitmap ConvertToBitmap(this byte[] imageBytes)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream(imageBytes))
				{
					return (Bitmap)Image.FromStream(bitmapStream);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(string.Format(BASE64_CONVERSION_ERROR_TEMPLATE, ex.Message));
				return null;
			}
		}


		public static string ConvertToBase64(this Bitmap bitmap)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream())
				{
					bitmap.Save(bitmapStream, ImageFormat.Bmp);
					return Convert.ToBase64String(bitmapStream.ToArray());
				}
			}
			catch (Exception ex)
			{
				Logger.LogError
				(
					string.Format
					(
						BITMAP_BASE64_CONVERSION_ERROR_TEMPLATE,
						ex.Message
					)
				);

				return null;
			}
		}


		public static byte[] ConvertToByteArray(this Bitmap bitmap)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream())
				{
					bitmap.Save(bitmapStream, ImageFormat.Bmp);
					return bitmapStream.ToArray();
				}
			}
			catch (Exception ex)
			{
				Logger.LogError
				(
					string.Format
					(
						BITMAP_BYTEARRAY_CONVERSION_ERROR_TEMPLATE,
						ex.Message
					)
				);

				return null;
			}
		}


		public static string CleanseBase64(this string base64)
		{
			return base64.Replace("\r\n", "").Replace(" ", "");
		}
		#endregion
	}
}
