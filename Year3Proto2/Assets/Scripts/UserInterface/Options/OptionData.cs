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

public interface OptionData<T>
{
    /**************************************
    * Name of the Function: SetData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: Template Data
    ***************************************/
    T GetData();

    /**************************************
    * Name of the Function: SetData
    * @Author: Tjeu Vreeburg
    * @Parameter: Template Data
    * @Return: void
    ***************************************/

    void SetData(T _data);

    /**************************************
    * Name of the Function: Deserialise
    * @Author: Tjeu Vreeburg
    * @Parameter: Template Data
    * @Return: void
    ***************************************/
    void Deserialise(T _data);

    /**************************************
    * Name of the Function: Serialise
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    void Serialise();
}