namespace mathexp_parser;


public class StackInvalidOperationException : Exception
{
    public StackInvalidOperationException(string message) : base(message)
    {
        
    }
}



public class CustomStack<T>
{
    private T[] _array;
    private int _size;
    private int _capacity;
    private const int InitialCapacity = 2;
    public int Count => _size;
    
    
    public CustomStack()
    {
        _array = new T[InitialCapacity];
        _capacity = InitialCapacity;
        _size = 0;
    }
    public CustomStack(int capacity)
    {
        _array = new T[capacity];
        _size = 0;
        _capacity = capacity;
    }

    public T Peek()
    {
        if (_size > 0)
        {
            return _array[_size - 1];
        }

        throw new StackInvalidOperationException("Stack is empty!");
    }

    public void Push(T element)
    {
        Console.WriteLine(_size);
        Console.WriteLine(_capacity);
        if (_size >= _capacity)
        {
            _capacity *= 2;
            _array = new T[_capacity];
        }
        _array[_size++] = element;
    }

    public T Pop()
    {
        if (_size > 0)
        {
            return _array[--_size];
        }
        throw new StackInvalidOperationException("Stack is empty!");
    }

    public bool TryPeek(out T? result)
    {
        try
        {
            result = Peek();
            return true;
        }
        catch (StackInvalidOperationException)
        {
            result = default;
        }

        return false;
    }
}