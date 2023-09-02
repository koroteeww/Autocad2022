#игра в угадайку.
#копмьютер загадает число от 0 до 500
#нам надо его угадать за минимальное количество шагов

import random  # import the random module
import json

scores = {}  # create an empty dictionary to store scores

def play_game():  # define a function called play_game
    name = input("Введи имя: ")  # ask the user to enter their name
    number = random.randint(0, 500)  # generate a random number between 0 and 500
    guess = -1  # initialize the guess variable to -1
    attempts = 0  # initialize the attempts variable to 0
    
    while guess != number:  # start a loop that continues until the user guesses the correct number
        guess = int(input("Угадай число от 0 до 500: "))  # ask the user to enter a guess and convert it to an integer
        attempts += 1  # increment the attempts variable by 1
        
        if guess < number:  # if the guess is too low
            print("Загаданное мной число БОЛЬШЕ!")
        elif guess > number:  # if the guess is too high
            print("Загаданное мной число МЕНЬШЕ!")
    
    print(f"Поздравляю {name}! Ты угадал число за {attempts} попыток.")  # print a congratulatory message with the user's name and number of attempts
    
    if name in scores:  # if the user has played before
        scores[name].append(attempts)  # add their score to their existing scores
    else:  # if the user is playing for the first time
        scores[name] = [attempts]  # create a new entry for them in the scores dictionary
        
    play_again = input("Сыграем еще? (да/нет) ")  # ask the user if they want to play again
    
    if play_again.lower() == "да":  # if the user wants to play again
        play_game()  # call the play_game function 
    else:  # if the user doesn't want to play again
        print("Thanks for playing!")  # print a message thanking the user for playing
        print(scores)  # print the scores dictionary
        json_string  = json.dumps(scores)
        with open('file.json',"w") as f:
            f.write(json_string)
            
# call the play_game function to start the game
play_game()  