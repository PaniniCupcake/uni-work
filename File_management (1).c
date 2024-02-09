#include <stdio.h>
#include <string.h>
#include <stdbool.h>
#include <unistd.h>
#include <dirent.h>
char str_log[100][37];
char str_log_2[100][37];
int type_log[100];
int line_log[100];
char undo_log[100][7]; //Stores whether the action was undone
int action_counter = -98; //Allows the log to be numbered
int log_pointer = 0; //These variables take information about the actions

int undo_pointer = 0; //Stores the location in the undo arrays
char undo_remake_names[5][37];//Stores the files to be remade upon undo
char undo_delete_names[5][37];//Stores the files to be deleted on undo
int undo_types[5];//Stores the type of undoable files for the log
int undo_lines[5];//Stores the lines of undoable files for the log
int available_undos = 0; //Stores how many undos can be used

char temp_file_name[37] = "TemporaryFileWithOverlyLongNameX.txt";//Stored this string in a variable so that I don't have to define it over and over

bool checkFileValid(char str[37])
{
	int i;
	if(strlen(str) > 20)//No files over 20 chars in length
	{
		return false;
	}
	for(i = 0;i <= 16; i++)
	{
		if(str[i] == '!' || str[i] == '@' || str[i] == '#' || str[i] == '$' || str[i] == '%' || str[i] == '^' || str[i] == '&' || str[i] == '*' || str[i] == '(' || str[i] == ')' || str[i] == '{'  || str[i] == '}' || str[i] == '[' || str[i] == ']' || str[i] == ':'|| str[i] == ';' || str[i] == '"' || str[i] == '\'' || str[i] == '<' || str[i] == '>' || str[i] == '/' || str[i] == '?' || str[i] == '~' || str[i] == '`' || str[i] == ' ')
		{//Rejects files with these chars in
			return false;
		}
		else if(str[i] == '.')//Only allows full stops if they are succeeded by .txt
		{
			if(!(str[i+1] == 't' && str[i+2] == 'x' && str[i+3] == 't' && strlen(str) == i+4))
			{
				return false;
			}
			return true;
		}
		else if(i == 16) //If .txt hasn't appeared yet then it won't be in the file name
		{
			return false;
		}
	}
	printf("Something went wrong. \n");//This should never occur
	return true;
}

void copyFile(char str[37],char str2[37])
{
	char c;
	FILE *file1;
	FILE * file2;
	file1 = fopen(str, "r");
	file2 = fopen(str2, "w");
	while( ( c = fgetc(file1) ) != EOF )//Puts every character from the first file into the second 
	{
	    fputc(c, file2);
	}
	fclose(file1);
	fclose(file2);
}

void appendLine(char str[37],int lines)
{
	temp_file_name[31] = 65 + undo_pointer;
	copyFile(str,temp_file_name); //Stores the array's previous contents into one of the 5 temp files			
	FILE *file1;
	char new_line[1024];
	printf("Enter the line you want to append to the current file.\n");
	gets(new_line);
	file1 = fopen(str, "a");
	if(lines > 0)
	{
		fprintf(file1, "\n"); //Starts a new line in the file
	}
	fputs(new_line, file1); //Puts the new line into the file
	fclose(file1);
}

void insertLine(char str[37], int line)
{
	int i = 0;
	char c;
	char new_line[1024];
	FILE *file1;
	FILE *file2;
	temp_file_name[31] = 65 + undo_pointer;
	copyFile(str,temp_file_name);
	
	file1 = fopen(temp_file_name, "r");
	file2 = fopen(str, "w");
	printf("Enter the line you want to add to the file. (Max length 1024).\n");
	gets(new_line);
	if(line == 0)//Appens the first line, as there won't be a \n on it
	{
		fputs(new_line,file2);
		fprintf(file2, "\n");
		i = 2;
	}
	while( ( c = fgetc(file1) ) != EOF )
	{
		if(c == '\n')
		{
			i ++;
		}
		fputc(c, file2);
		if(i == line)//Appends the new line when that line is reached in the file
		{
			fputs(new_line,file2);
			fprintf(file2, "\n");
			i++; //Increments i so new_line won't be added more than once.
		}
	}
	fclose(file1);
	fclose(file2);
}

void deleteLine(char str[37], int line)
{
	int i = 0;
	char c;	
	FILE *file1;
	FILE * file2;
	temp_file_name[31] = 65 + undo_pointer;
	copyFile(str,temp_file_name);
	
	file1 = fopen(temp_file_name, "r");
	file2 = fopen(str, "w");
	
	while( ( c = fgetc(file1) ) != EOF )//Loops through every character in the file until the end is reached
	{
		if(c == '\n')
		{
			i ++;
		}
		if(i != line)//Neglects adding characters to the deleted line
		{
			fputc(c, file2);
		}
	}
	fclose(file1);
	fclose(file2);
}

int getFileLength(char str[37])
{
	FILE *file1;
	char c;
	int i = 0;
	file1 = fopen(str, "r");
	fseek(file1, 0, SEEK_SET);
	while (( c = fgetc(file1)) != EOF) //Loops through every character in the file until the end is reached
	{
		printf("Adad %d \n",i);
		if(i == 0)//The first line may not have a \n, so should be added if the first character isn't the end of the file
		{
			printf("I 0\n");
			i++;
		}
		if (c == '\n')//Counts every new line in the file
		{
			i++;
		}
		c = getc(file1);
	}
	return i;
}

void updateLog(int type, char str[37], char str2[37], int line, int undo) 
{
	action_counter ++;
	type_log[log_pointer] = type;
	strcpy(str_log[log_pointer], str);
	strcpy(str_log_2[log_pointer], str2);
	line_log[log_pointer] = line;
	if(undo == 0)
	{
		strcpy(undo_log[log_pointer],"");
	}
	else if(undo == 1)
	{
		undo_log[log_pointer][0] = '\0';//Clears the current contents of the undo_log
		strcpy(undo_log[log_pointer],"Undid: ");
	}
	log_pointer ++;
	log_pointer %= 100;
	//Updates the contents of the log at the current pointer, then increments the pointer, overwriting previous ones if there are over 100
}

void printLog()
{
	int i = 0;
	printf("The log currently contains: (Note that the log only stores the past 100 actions)\n\n");
	for(i = log_pointer + 1; true;  i ++)//Prints the last 100 actions taken.
	{
		i %= 100;
		switch(type_log[i])
		{
			case -1:
				break; //Outputs nothing if there is no type
			case 0://Otherwise, outputs the details specific to each action type, along with whether it was undone
				printf("%d: File %s opened.\n",(action_counter + (i + 99 - log_pointer) % 100),str_log[i]);
				break;
			case 1:
				printf("%d: %sFile %s created.\n",action_counter +((i + 99 - log_pointer) % 100),undo_log[i],str_log[i]);
				break;
			case 2:
				printf("%d: %sFile %s copied into file %s\n",(action_counter +(i + 99 - log_pointer) % 100),undo_log[i],str_log[i],str_log_2[i]);
				break;
			case 3:
				printf("%d: %sFile %s deleted.\n",(action_counter +(i + 99 - log_pointer) % 100),undo_log[i],str_log[i]);
				break;
			case 4:
				printf("%d: File %s displayed.\n",(action_counter +(i + 99 - log_pointer) % 100),str_log[i]);
				break;
			case 5:
				printf("%d: %sLine appended to file %s\n",(action_counter +(i + 99 - log_pointer) % 100),undo_log[i],str_log[i]);
				break;
			case 6:
				printf("%d: Number of lines in file %s displayed\n",(action_counter +(i + 99 - log_pointer) % 100),str_log[i]);
				break;
			case 7:
				printf("%d: %sDeleted line %d of file %s\n",(action_counter +(i + 99 - log_pointer) % 100),undo_log[i],line_log[i],str_log[i]);
				break;
			case 8:
				printf("%d: %sInserted line at line %d of file %s\n",(action_counter +(i + 99 - log_pointer) % 100),undo_log[i],line_log[i],str_log[i]);
				break;
			case 9:
				printf("%d: Displayed line %d of file %s\n",(action_counter +(i + 99 - log_pointer) % 100),line_log[i],str_log[i]);
				break;
			case 10:
				printf("%d: Displayed the action log.\n",(action_counter +(i + 99 - log_pointer) % 100));
				break;
		}
		if(i == log_pointer)
		{
			break;
		}
	}
	printf("\n\n");
}

void updateUndos(int type, char remake[37], char dele[37], int line)
{
	if(available_undos < 5)
	{
		available_undos ++;
	}
	strcpy(undo_remake_names[undo_pointer], remake);
	strcpy(undo_delete_names[undo_pointer], dele);
	undo_types[undo_pointer] = type;
	undo_lines[undo_pointer] = line;
	undo_pointer ++;
	undo_pointer %= 5;
	//Updates the contents of the undo log and reduces the available undos, overwriting undos five actions old
}

bool undo()
{
	if(available_undos > 0) //The program only stores 5 undos, but can be modified to fit more
	{
		available_undos --;
		undo_pointer = (undo_pointer + 4) % 5;
		temp_file_name[31] = 65 + undo_pointer;
		if(!strcmp(undo_delete_names[undo_pointer],"") == 0) //Deletes a file designated for deletion
		{
			remove(undo_delete_names[undo_pointer]);
		}
		if(!strcmp(undo_remake_names[undo_pointer],"") == 0)//Remakes a file designated for restoration by copying it from the temp file storing it
		{ 
			copyFile(temp_file_name, undo_remake_names[undo_pointer]);
		}
		updateLog(undo_types[undo_pointer],"","",0,1);
		if(undo_types[undo_pointer] == 1)//Returns to the selection if you undid the creation of a file.
		{
			printf("Undo successful. Returning to file selection.\n");
			return true;
		}
		printf("Undo successful.\n");
	}
	else
	{
		printf("You can't undo any further.\n");
	}
	return false;
}




int main()
{
	char c;
	
	int i; //Used for loops and temporary storage of int values
	int j;
	int option = 0;
	
	FILE *file1;
	FILE *file_temp[5];	//Five files that store the contents of files that have been modified
	
	DIR *directory;//Stores the current directory
	struct dirent *dir;
	
	char str[37];
	char str2[37];
	
	bool close_file = false; //Stores whether the file is to be exited
	
	for(i = 0; i < 100; i++)
	{
		type_log[i] = -1;
	}//Makes sure the types are all nothing original
	
	for(i = 0; i < 5; i++)
	{
		//Generates 5 uniquely named strings for the temp files. If I needed more undos then there are characters, I could just have two characters to change in the temp_file_name string
		temp_file_name[31] = 65 + i;
		if(access(temp_file_name, F_OK) == 0) 
		{
		    printf("You already have a file named TemporaryFileWithOverlyLongNameX.txt in this directory (You probably didn't close the file properly last run). Please delete or rename any files with the name.");
		    return 0;
		}
		file_temp[i] = fopen(temp_file_name,"w");
		fclose(file_temp[i]);
		strcpy(undo_remake_names[i], ""); //Empties the undo files
		strcpy(undo_delete_names[i], "");
	}
	
	
	printf("\nNote: In any menu, you can type 'Undo' to undo an action, 'Redo' to redo an action, 'Log' to display the action log, or 'Quit' to close the manager.\n\n"); //Informs the user of universal operations
	
	//Start of first menu
	
	while(true)//Loops through this menu until a valid file name is entered
	{
		close_file = false;
		printf("File name entry:\nPlease enter the name of the text file you wish to create and/or manage.\n");
		
		printf("This directory currently contains:\n\n");	
    	directory = opendir(".");//Opens the current directory
    	if (directory)
    	{
       		while ((dir = readdir(directory)) != NULL)//Checks the contents of the directory until there are none left
		    {
		    	if(checkFileValid(dir->d_name))//Filters out the files that match the editing criteria
		    	{
		        	printf("%s\n", dir->d_name);
		        }
		    }
	        closedir(directory);
		}
		printf("\n");
		//Displays the contents of the directory so that the user can see it
		
		printf("(New file names must contain no non-alphanumeric characters other than - or _, end with .txt and not exceed 20 characters, including the extension.)\n");
		gets(str); 
		printf("\n");
		if(strcmp(str,"Quit") == 0) //Closes the document if there is nothing left to say
		{
			break;
		}
		else if(strcmp(str,"Undo")== 0)
		{
			undo();
			continue;
		}
		else if(strcmp(str,"Log")== 0)
		{
			printLog();
			updateLog(10,"","",0,0);
			continue;
		}
		if(!checkFileValid(str))
		{
			printf("Invalid input. Inputs must not exceed 20 characters, end with .txt and contain no alphanumeric characters other than - or _\n\n");
			continue;
		}
		else if(strlen(str) < 5)
		{
			printf("Your file doesn't have a name.\n");
			continue;
		}
		if(access(str, F_OK) == 0) //Type 0
		{//If the file exists, does nothing but update the log as the str already stores the information we need.
			printf("Successfully opened file. \n\n");
			updateLog(0,str,"",0,0);
		}
		else //Type 1
		{//Opens and closes the file so that it is created if the file doesn't already exist
			printf("Successfully created and opened file. \n\n");
			updateLog(1,str,"",0,0);
			updateUndos(1,"",str,0);
			file1 = fopen (str, "w");
			fclose(file1);
		}
		
		//Start of second menu
		
		while(true)//Opens a second menu when a file has been selected
		{
			printf("File operations:\nEnter 'Copy' to copy this file's contents to a different file, 'Delete' to delete this file, 'Show' to view the contents of the current file, 'Lines' to perform line operations, or 'Return' to go back to file selection.\n");
			gets(str2);
			printf("\n");
			if(strcmp(str2,"Quit") == 0)
			{//Closes the menu and proceeds to other Quit statements that close the application
				break;
			}
			else if(strcmp(str2,"Copy") == 0) //Type 2
			{
				while(true)
				{
					printf("Enter the name of the file you want to copy the current file's contents to (Remember, it must follow the same naming convention). Enter 'Return' to go back to file operations.\n");
					gets(str2);
					if(strcmp(str2,"Return") == 0)
					{
						break;
					}
					if(!checkFileValid(str2))//Uses the same file validator as the main file entry
					{
						printf("Invalid input. Inputs must not exceed 20 characters, end with .txt and contain no alphanumeric characters other than - or _\n\n");
						continue;
					}
					else if(strlen(str) < 5)
					{
						printf("Your file doesn't have a name.\n");
						continue;
					}
					if(access(str2, F_OK) == 0) //IF the file already exists, does a double check in case the user didn't realise it already existed
					{
						while(true)
						{
							printf("The file %s exists. Are you sure you want to overwrite it (Y/N)?\n",str2);
							scanf("%c",&c);
							fflush(stdin);
							if(c == 'Y')
							{
								temp_file_name[31] = 65 + undo_pointer; //Remembers the original file in case it is undone
								copyFile(str2,temp_file_name);	
								copyFile(str,str2);
								updateLog(2,str,str2,0,0);
								updateUndos(2,str2,"",0);//If the previous file did exist, overwrite the new file with the old one upon undo
								break;
							}
							else if (c == 'N') //Does nothing if the user changes their mind
							{
								printf("The file has not been copied.\n\n");
								break;
							}
							else
							{
								printf("Please type Y for yes or N for no.\n\n");//Loops until yes or no is given
							}
						}
					}
					else
					{
						copyFile(str,str2);
						updateLog(2,str,str2,0,0);
						updateUndos(2,"",str2,0); //If the previous file didn't exist, just delete the new file upon undo
					}
					break;
				}
			}
			else if(strcmp(str2,"Delete") == 0) //Type 3
			{	
				temp_file_name[31] = 65 + undo_pointer;
				copyFile(str,temp_file_name);	//Stores the file in case it needs undoing
				updateLog(3,str,"",0,0);
				updateUndos(3,str,"",0);	
				remove(str); //Deletes the file
				printf("File succesfully deleted.\n\n");
				break;
			}
			else if(strcmp(str2,"Show") == 0) //Type 4
			{
				file1 = fopen (str, "r");
				fseek (file1, 0, SEEK_END);
			    i = ftell(file1);
			    if (i == 0) 
				{
			        printf("This file contains nothing.\n");
			    }
			    else
			    {
			    	printf("This file contains: \n\n");
					fseek(file1, 0, SEEK_SET);
					while( ( c = fgetc(file1) ) != EOF )//Prints every character until the end of the file
					{
					    printf("%c",c);
					}
					printf("\n\n");
				}
				updateLog(4,str,"",0,0);
			}
			else if(strcmp(str2,"Lines") == 0)
			{
				
				//Start of 3rd menu
				
				while(true)//Starts a new menu for line operations.
				{
					printf("Line operations menu A:\nEnter 'Append' to add a line to the end of the file, 'Count' to display how many lines are in the file, 'More' to perform operations at specific places in the file, or 'Return' to go back.\n");
					gets(str2);
					printf("\n");
					if(strcmp(str2,"Quit") == 0)
					{
						break;
					}
					if(strcmp(str2,"Append") == 0) //Type 5
					{		
						appendLine(str,getFileLength(str));//Calls append function and updates the logs
						updateLog(5,str,"",0,0);
						updateUndos(5,str,str,0);

					}
					else if(strcmp(str2,"Count") == 0) //Type 6
					{
						printf("This file contains %d lines.\n", getFileLength(str));
						updateLog(6,str,"",0,0);
					}
					else if(strcmp(str2,"More") == 0)
					{
						
						//Start of 4th menu
						
						while(true)//New menu for line specific line operations
						{
							fflush(stdin);
							printf("Enter the line number of the line you want to perform operations at.\n");
							if(scanf("%d%c",&i,&c) == 2 && c == '\n' && i > 0 && i < getFileLength(str) + 1)//Makes sure the int input is valid and of correct length
							{
								printf("\n");
								while(true)
								{
									printf("Line operations menu B:\nCurrent line : %d \nEnter 'Delete' to delete the current line, 'Insert' to add a new line here, 'Show' to show the current line, 'Change' to change the current line, or 'Return' to go back.\n",i);
									gets(str2);
									printf("\n");
									if(strcmp(str2,"Quit") == 0)
									{
										break;
									}
									else if(strcmp(str2,"Delete") == 0) //Type 7
									{
										deleteLine(str,i-1);//Runs the delete funciton and updates the logs
										updateLog(7,str,"",i,0);
										updateUndos(7,str,str,i);
										printf("Line deleted.\n\n");
									}
									else if(strcmp(str2,"Insert") == 0) //Type 8
									{
										insertLine(str,i-1);//Runs the insert funciton and updates the logs
										updateLog(8,str,"",i,0);
										updateUndos(8,str,str,i);
										printf("Line Inserted.\n\n");
									}
									else if(strcmp(str2,"Show") == 0) //Type 9
									{
										file1 = fopen(str,"r");
										printf("%d\n",i);
										j = 0;
										while(( c = fgetc(file1)) != EOF)//Loops through the file and only prints characters that are on the desired line.
										{
											if(c ==  '\n')
											{
												j ++;
											}
											else if(i - 1 == j)
											{
											    printf("%c",c);
											}
										}
										fclose(file1);
										printf("\n\n");
										updateLog(9,str,"",i,0);
									}
									else if(strcmp(str2,"Return") == 0 || strcmp(str2,"Change") == 0)
									{
										break;
									}
									else if(strcmp(str2,"Undo")== 0)
									{
										close_file = undo();
										if(close_file)//Closes this file if it has been deleted by undoing its creation
										{
											break;
										}
									}
									else if(strcmp(str2,"Log")== 0)
									{
										printLog(); 
										updateLog(10,"","",0,0);
									}
									else
									{
										printf("Invalid input.\n\n");
									}
								}
								if(strcmp(str2,"Change") != 0 || strcmp(str2,"Delete") != 0)
								{
									//Goes back to line selection if the line doesn't exist or if the user wants to change this line
									break;
								}
							}	
							else 
							{
								printf("\nThe line number must be an integer greater than 0 and lower than the file's length (%d). If you want to add a line at the end of the file, use append.\n\n",getFileLength(str));
							}
						}
						
						//End of 4th menu
					}
					else if(strcmp(str2,"Return") == 0)
					{
						break;
					}
					else if(strcmp(str2,"Undo")== 0)
					{
						close_file = undo();
					}
					else if(strcmp(str2,"Log")== 0)
					{
						printLog();
						updateLog(10,"","",0,0);
					}
					else
					{
						printf("Invalid input.\n\n");
					}
					if(strcmp(str2,"Quit") == 0||close_file)
					{//Exits to file selection. 
						break;
					}
					
					//End of 3rd menu
					
				}
			}
			else if(strcmp(str2,"Return") == 0)
			{
				break;
			}
			else if(strcmp(str2,"Undo")== 0)
			{
				close_file = undo();
			}
			else if(strcmp(str2,"Log")== 0)
			{
				printLog();
				updateLog(10,"","",0,0);
			}
			else
			{
				printf("Invalid input.\n\n");
			}
			if(strcmp(str2,"Quit") == 0 || close_file)
			{//Exits to file selection. 
				break;
			}
			
			//End of 2nd menu
		}
		if(strcmp(str2,"Quit") == 0)
		{//Closes the file if Quit has been entered somewhere
			break;
		}
		
		//End of 1st menu
		
	}
	for(i = 0; i < 5; i++)
	{
		temp_file_name[31] = 65 + i;
		remove(temp_file_name);
		//Removes the temp files, as they are no longer needed and are unwanted
	}
}
