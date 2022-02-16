using System.Collections.Generic;

#pragma warning disable CS8618

namespace RichEntity.Analyzers.Sample.Types;

public class Proper
{
    public class Person
    {
        private readonly List<Person> _friends;
        private readonly List<Person> _children;
        private readonly Person? _father;
        private readonly string _name;

        public Person(string name, Person? father)
        {
            _children = new List<Person>();
            _friends = new List<Person>();
            _name = name;
            _father = father;
        }

        protected Person() { }

        public string Name => _name;
        public Person? Father => _father;

        public IReadOnlyCollection<Person> Children => _children;

        public void AddChild(Person person)
            => _children.Add(person);

        public void AddFriend(Person person)
            => _friends.Add(person);
    }
}