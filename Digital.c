#include <stdio.h>
//The function works by starting from the lowest digit, then adding higher digits until the total is reached. The digits are then added to the total for as many times as they had
//numbers with a correct digital sum for.
long long unsigned total = 0; //Totals up the values given by the function
void digitalSum(int n, int current_val, long long unsigned multiplier,long long unsigned current_num)
{//The function takes parameters n, the number we are finding the total digital sums of, current_val, the current digital sum of the number, multiplier, the 10 power required for the next number to be in the next digit, 
//and current_num, the number that we are building into a number with a digital sum of n
	int i;
	for(i = 1; i < 10; i ++)
	{//Starts at the lowest digit, and works its way up.
		if(current_val + i == n)
		{
			total += i * multiplier + current_num; //When the digital sum is found to be n, we add it to the total
			break; //If this i makes the digital sum correct, the higher i won't.
		}
		digitalSum(n,current_val + i,multiplier * 10,current_num + i * multiplier); //Finds out how many solutions there are that end with the current combination of digits, including i
	}
}
int main()
{
    int n = 0;
    char error_check;
    printf("Enter an integer between 0 and 20 you want the digital sum of.\n");
    if(scanf("%d%c",&n,&error_check) != 2 || error_check != '\n' || n > 19 || n < 0) //Returns the error message unless the input is valid.
    {
    	printf("\nPlease enter an integer between 0 and 20.");
	}
	else
	{
		digitalSum(n,0,1,0);
    	printf("\nThe digital sum is %llu. \n",total);
	}
}
