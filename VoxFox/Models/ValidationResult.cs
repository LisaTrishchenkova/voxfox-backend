namespace VoxFox.Models;

public class ValidationResult
{
	public bool IsSuccess { get; set; }
	public string Error { get; set; }
	public int StatusCode { get; set; }

	public static ValidationResult Success()
	{
		return new ValidationResult { IsSuccess = true };
	}

	public static ValidationResult Fail(string error, int statusCode = 400)
	{
		return new ValidationResult
		{
			IsSuccess = false,
			Error = error,
			StatusCode = statusCode
		};
	}
}
