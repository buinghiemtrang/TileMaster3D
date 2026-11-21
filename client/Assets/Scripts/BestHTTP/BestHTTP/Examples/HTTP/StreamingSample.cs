using BestHTTP.Examples.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.HTTP
{
	public class StreamingSample : SampleBase
	{
		[Tooltip("The url of the resource to download")]
		[SerializeField]
		protected string _downloadPath;

		[Header("Streaming Setup")]
		[SerializeField]
		protected RectTransform _streamingSetupRoot;

		[SerializeField]
		protected Slider _fragmentSizeSlider;

		[SerializeField]
		protected Text _fragmentSizeText;

		[SerializeField]
		protected Toggle _disableCacheToggle;

		[Header("Reporting")]
		[SerializeField]
		protected RectTransform _reportingRoot;

		[SerializeField]
		protected Slider _downloadProgressSlider;

		[SerializeField]
		protected Text _downloadProgressText;

		[SerializeField]
		protected Slider _processedDataSlider;

		[SerializeField]
		protected Text _processedDataText;

		[SerializeField]
		protected Text _statusText;

		[SerializeField]
		protected Button _startDownload;

		[SerializeField]
		protected Button _cancelDownload;

		protected HTTPRequest request;

		protected float progress;

		protected int fragmentSize;

		protected virtual long DownloadLength { get; set; }

		protected virtual long ProcessedBytes { get; set; }

		protected override void Start()
		{
		}

		protected void OnDestroy()
		{
		}

		public void OnFragmentSizeSliderChanged(float value)
		{
		}

		public void Cancel()
		{
		}

		protected virtual void SetupRequest()
		{
		}

		public virtual void StartStreaming()
		{
		}

		private void OnHeadersReceived(HTTPRequest req, HTTPResponse resp)
		{
		}

		protected virtual void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
		{
		}

		protected virtual void OnDownloadProgress(HTTPRequest originalRequest, long downloaded, long downloadLength)
		{
		}

		protected virtual bool OnDataDownloaded(HTTPRequest request, HTTPResponse response, byte[] dataFragment, int dataFragmentLength)
		{
			return false;
		}

		protected void SetDataProcessedUI(long processed, long length)
		{
		}

		protected virtual void ResetProcessedValues()
		{
		}
	}
}
