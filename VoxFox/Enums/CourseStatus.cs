namespace VoxFox.Enums
{
    public enum CourseStatus
    {
        Draft = 1, //создан, не отправлен
        UnderReview = 2, //отправлен, у модератора
        RejectedByModerator = 3, //отклонен - можно исправить и отправить снова
        Published = 4 //одобрен и опубликован модератором
    }
}
