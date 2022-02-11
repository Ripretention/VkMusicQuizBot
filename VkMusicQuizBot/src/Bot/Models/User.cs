using System;

namespace VkMusicQuizBot
{
    public class User : IUser, IEquatable<User>
    {
        public int Id { get; set; }
        public uint Score { get; set; } = 0;
        public UserAccess Access { get; set; } = UserAccess.Default;
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
        public override int GetHashCode() => Id;
        public override string ToString() =>
            $"{(Id < 0 ? "Group" : "User")}-{Math.Abs(Id)}";
    }

    public interface IUser
    {
        public int Id { get; set; }
        public uint Score { get; set; }
        public UserAccess Access { get; set; }
        public string GetAppeal(string label);
    }

    public enum UserAccess
    {
        Banned, Default, Administration, Owner
    }
}
