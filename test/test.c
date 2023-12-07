int main()
{
    return method();
}

__attribute__ ((noinline))
int method() 
{
    long time = 60000000;
    long distance = 450000000000000;
    
    long wins = 0;
    for (long i = 0; i <= time; i++)
    {
        if (((time - i) * i) > distance)
        {
            wins++;
        }
    }

    return (int)wins;
}