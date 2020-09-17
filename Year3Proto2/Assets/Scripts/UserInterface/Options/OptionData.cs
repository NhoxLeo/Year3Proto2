// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : OptionObject.cs
// Description  : Contains interface for data manipulation along with base class for objects.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com
public delegate void OptionCallback();
public interface OptionDataBase { }

public class OptionData 
{
    protected OptionCallback optionCallback;

    public OptionCallback GetCallback()
    {
        return optionCallback;
    }
}
