namespace VoxFox.Enums
{
	public enum CourseStatus
	{
		Draft = 1,                  // создан, не отправлен
		UnderReview = 2,            // отправлен, у модератора
		RejectedByModerator = 3,    // отклонён — можно исправить и отправить снова
		Published = 4,              // одобрен и опубликован
		PublishedUnderReview = 5    // опубликован, но отправлен на повторную модерацию — виден в каталоге
	}
}
