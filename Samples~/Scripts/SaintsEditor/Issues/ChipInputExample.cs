using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class ChipInputExample : SaintsMonoBehaviour
    {
        [Serializable]
        public enum Equipment
        {
            Special,

            [InspectorName("Weapon/Sword")]
            Sword,

            [InspectorName("Weapon/Bow")]
            Bow,

            [InspectorName("Armor/Shield")]
            Shield,
        }

        // [LayoutStart("x", ELayout.CollapseBox)]

        // [ListDrawerSettings(numberOfItemsPerPage: 2)]
        // public Equipment[] defaultList;

        // [Separator(20)]

        [Chips] public Equipment[] normalList;
        // duplicated options are disabled
        [Chips(EUnique.Disable)] public Equipment[] disableList;
        // duplicated options are removed
        [Chips(EUnique.Remove)] public Equipment[] removeList;

        [Chips(nameof(GetTags), slashAsSub = false)]  // callback
        public List<string> tags;

        private IEnumerable<string> GetTags() => new[]
        {
            "Player",
            "Enemy/Boss",
            "Environment",
        };

        [Separator(20)]

        // [AboveText("This is a long driver for people have nothing to think about")]
        [AboveText("<label/>")]
        [LabelText(null)]
        [Chips(nameof(GetLongList))]
        public List<string> longList;

        private IEnumerable<string> GetLongList()
        {
            for (int index = 1; index <= 100; index++)
            {
                yield return $"Long List Item {index:000}";
            }
        }

        [Chips(nameof(GetEnvAsync))]  // async
        public List<int> envLayer;

        private IEnumerator GetEnvAsync()
        {
            yield return new WaitForSeconds(2);

            yield return new Dropdown<int>()
            {
                { "Basic", 1 },
                { "Advanced/MiniBoss", 2 },
                { "Advanced/Boss", 2 },
                { "Final/Boss", 3 },
            };
        }

        [Chips(nameof(GetGo))]
        public List<Component> dragNDrop;

        private IEnumerable<Component> GetGo()
        {
            return GetComponentsInChildren<Component>(true);
        }

        // [LayoutEnd]

        // icon
        [Chips(nameof(GetIcons))]
        public List<int> withIcons;

        private Dropdown<int> GetIcons()
        {
            return new Dropdown<int>
            {
                { "Search", 1, false, "search.png", Color.cyan },
                { "Play", 2, true, "play.png" },
                { "Star", 3, false, "star.png", EColor.Gold.GetColor() },
                { "Pencil", 4, false, "pencil.png" },
            };
        }

        [Serializable]
        public struct TopFilm : IEquatable<TopFilm>
        {
            public string title;
            public int year;

            public bool Equals(TopFilm other)
            {
                return string.Equals(title, other.title, StringComparison.Ordinal) && year == other.year;
            }

            public override bool Equals(object obj)
            {
                return obj is TopFilm other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((title == null ? 0 : StringComparer.Ordinal.GetHashCode(title)) * 397) ^ year;
                }
            }

            public static bool operator ==(TopFilm left, TopFilm right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TopFilm left, TopFilm right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"{title}({year})";
            }
        }

        [Chips(nameof(PickFilmWhileTyping))] public TopFilm[] pickFilmWhileTyping;

        private IEnumerable<TopFilm> PickFilmWhileTyping(string search)
        {
            string searchLow = search.Trim().ToLower();
            foreach (TopFilm topFilm in TopFilms)
            {
                if (topFilm.title.ToLower().Contains(searchLow))
                {
                    yield return topFilm;
                }
            }
        }

        // If your resource need prepare, you can also wait
        [Chips(nameof(PickFilmWhileTypingWait))] public TopFilm[] pickFilmWhileTypingWait;

        private async Task<Dropdown<TopFilm>> PickFilmWhileTypingWait(string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                await Task.Delay(2000);  // prepare your resource and wait for the result
            }

            Dropdown<TopFilm> result = new Dropdown<TopFilm>();
            foreach (TopFilm each in PickFilmWhileTyping(search))
            {
                result.Add(each.ToString(), each);
            }

            return result;
        }

        // Top films as rated by IMDb users. http://www.imdb.com/chart/top
        private static readonly TopFilm[] TopFilms =
        {
            new TopFilm { title = "The Shawshank Redemption", year = 1994 },
            new TopFilm { title = "The Godfather", year = 1972 },
            new TopFilm { title = "The Godfather: Part II", year = 1974 },
            new TopFilm { title = "The Dark Knight", year = 2008 },
            new TopFilm { title = "12 Angry Men", year = 1957 },
            new TopFilm { title = "Schindler's List", year = 1993 },
            new TopFilm { title = "Pulp Fiction", year = 1994 },
            new TopFilm { title = "The Lord of the Rings: The Return of the King", year = 2003 },
            new TopFilm { title = "The Good, the Bad and the Ugly", year = 1966 },
            new TopFilm { title = "Fight Club", year = 1999 },
            new TopFilm { title = "The Lord of the Rings: The Fellowship of the Ring", year = 2001 },
            new TopFilm { title = "Star Wars: Episode V - The Empire Strikes Back", year = 1980 },
            new TopFilm { title = "Forrest Gump", year = 1994 },
            new TopFilm { title = "Inception", year = 2010 },
            new TopFilm { title = "The Lord of the Rings: The Two Towers", year = 2002 },
            new TopFilm { title = "One Flew Over the Cuckoo's Nest", year = 1975 },
            new TopFilm { title = "Goodfellas", year = 1990 },
            new TopFilm { title = "The Matrix", year = 1999 },
            new TopFilm { title = "Seven Samurai", year = 1954 },
            new TopFilm { title = "Star Wars: Episode IV - A New Hope", year = 1977 },
            new TopFilm { title = "City of God", year = 2002 },
            new TopFilm { title = "Se7en", year = 1995 },
            new TopFilm { title = "The Silence of the Lambs", year = 1991 },
            new TopFilm { title = "It's a Wonderful Life", year = 1946 },
            new TopFilm { title = "Life Is Beautiful", year = 1997 },
            new TopFilm { title = "The Usual Suspects", year = 1995 },
            new TopFilm { title = "Léon: The Professional", year = 1994 },
            new TopFilm { title = "Spirited Away", year = 2001 },
            new TopFilm { title = "Saving Private Ryan", year = 1998 },
            new TopFilm { title = "Once Upon a Time in the West", year = 1968 },
            new TopFilm { title = "American History X", year = 1998 },
            new TopFilm { title = "Interstellar", year = 2014 },
        };
    }
}
