//using CleanBase.Core.Services.Storage;
//using Amazon;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Amazon.S3;
//using Amazon.S3.Model;

//namespace CleanBase.Core.Infrastructure.Storage.AWS
//{
//	public class S3StorageProvider : IStorageProvider
//	{
//		private readonly IAmazonS3 _s3Client;

//		public S3StorageProvider(IAmazonS3 s3Client)
//		{
//			_s3Client = s3Client;
//		}

//		public async Task<bool> CheckExistsAsync(string bucketName, string key)
//		{
//			try
//			{
//				var response = await _s3Client.HeadObjectAsync(new HeadObjectRequest
//				{
//					BucketName = bucketName,
//					Key = key
//				});
//				return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
//			}
//			catch (AmazonS3Exception e) when (e.ErrorCode == "NotFound")
//			{
//				return false;
//			}
//		}

//		public async Task<string> ReadFileAsStringAsync(string bucketName, string key)
//		{
//			using (var response = await _s3Client.GetObjectAsync(bucketName, key))
//			using (var reader = new StreamReader(response.ResponseStream))
//			{
//				return await reader.ReadToEndAsync();
//			}
//		}

//		public async Task<string> ReadFileAsBase64Async(string bucketName, string key)
//		{
//			using (var response = await _s3Client.GetObjectAsync(bucketName, key))
//			using (var memoryStream = new MemoryStream())
//			{
//				await response.ResponseStream.CopyToAsync(memoryStream);
//				return Convert.ToBase64String(memoryStream.ToArray());
//			}
//		}

//		public async Task<Stream> ReadFileAsStreamAsync(string bucketName, string key)
//		{
//			var response = await _s3Client.GetObjectAsync(bucketName, key);
//			return response.ResponseStream;
//		}

//		public async Task<string> UploadFileAsync(string filePath, string bucketName, string key)
//		{
//			var fileTransferUtility = new TransferUtility(_s3Client);
//			await fileTransferUtility.UploadAsync(filePath, bucketName, key);
//			return key;
//		}

//		public async Task<string> UploadBytesAsync(string bucketName, string key, byte[] data, string contentType = null)
//		{
//			using (var stream = new MemoryStream(data))
//			{
//				var uploadRequest = new PutObjectRequest
//				{
//					BucketName = bucketName,
//					Key = key,
//					InputStream = stream,
//					ContentType = contentType
//				};
//				await _s3Client.PutObjectAsync(uploadRequest);
//				return key;
//			}
//		}

//		public async Task<IDictionary<string, string>> GetMetadataAsync(string bucketName, string key)
//		{
//			var response = await _s3Client.GetObjectMetadataAsync(bucketName, key);
//			return response.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
//		}

//		public async Task SetMetadataAsync(string bucketName, string key, IDictionary<string, string> metadata)
//		{
//			var copyRequest = new CopyObjectRequest
//			{
//				SourceBucket = bucketName,
//				SourceKey = key,
//				DestinationBucket = bucketName,
//				DestinationKey = key,
//				Metadata = new Dictionary<string, string>(metadata),
//				MetadataDirective = S3MetadataDirective.REPLACE
//			};
//			await _s3Client.CopyObjectAsync(copyRequest);
//		}

//		public async Task<IEnumerable<string>> ListFilesAsync(string bucketName, string prefix = null)
//		{
//			var files = new List<string>();
//			var request = new ListObjectsV2Request
//			{
//				BucketName = bucketName,
//				Prefix = prefix
//			};

//			ListObjectsV2Response response;
//			do
//			{
//				response = await _s3Client.ListObjectsV2Async(request);
//				files.AddRange(response.S3Objects.Select(o => o.Key));
//				request.ContinuationToken = response.NextContinuationToken;
//			} while (response.IsTruncated);

//			return files;
//		}
//	}
//}
