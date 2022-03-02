using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace VkMusicQuizBot
{
    public class User : IUser, IEquatable<User>
    {
        [Key]
        public long Id { get; set; }
        public uint Score { get; set; } = 0;
        
        public UserAccess Access { get; set; } = UserAccess.Unconfirmed;
        public UserStatistic Statistic { get; set; } = new UserStatistic();
        public string GetAppeal(string label = "Пользователь") =>
            $"[{(Id < 0 ? "club" : "id")}{Math.Abs(Id)}|{label}]";

        public bool Equals(User other) =>
            other != null
                ? other.Id == Id
                : false;
        public override bool Equals(object obj) =>
            obj != null
                ? Equals(obj as User)
                : false;
        public override int GetHashCode() => (int)Id;
        public override string ToString() =>
            $"{(Id < 0 ? "Group" : "User")}-{Math.Abs(Id)}";
    }
    public class UserStatistic : IUserStatistic
    {
        [Key]
        public long Id { get; set; }
        public uint WinCount { get; set; } = 0;
        public uint LoseCount { get; set; } = 0;
        [NotMapped]
        public uint TotalCount { get => WinCount + LoseCount; }
        public override string ToString() =>
            TotalCount <= 0
                ? "неизвестна"
                : $"{WinCount:N0} побед / {TotalCount:N0} игр ({((float)WinCount/TotalCount):P} побед)";
    }

    public interface IUser
    {
        public long Id { get; set; }
        public uint Score { get; set; }
        public UserAccess Access { get; set; }
        public UserStatistic Statistic { get; set; }
        public string GetAppeal(string label);
    }
    public interface IUserStatistic
    {
        public long Id { get; set; }
        public uint WinCount { get; set; }
        public uint LoseCount { get; set; }
        public uint TotalCount { get => WinCount + LoseCount; }
    }
    public enum UserAccess
    {
        Banned, Unconfirmed, Default, Administration, Owner
    }
}
