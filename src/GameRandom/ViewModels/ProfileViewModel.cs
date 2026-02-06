using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GameRandom.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    public ObservableCollection<Person> People { get; set; }

    public ProfileViewModel()
    {
        var people = new List<Person> 
        {
            new Person("Neil", 27, "Male"),
            new Person("Buzz", 22, "Female"),
            new Person("James", 18, "Male"),
        };
        
        People = new ObservableCollection<Person>(people);
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }

    public Person(string name, int age, string gender)
    {
        Name = name;
        Age = age;
        Gender = gender;
    }
}