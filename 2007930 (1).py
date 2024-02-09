from os import X_OK
import comedian
import demographic
import ReaderWriter
import timetable
import random
import math

class Scheduler:

	def __init__(self,comedian_List, demographic_List):
		self.comedian_List = comedian_List
		self.demographic_List = demographic_List
		self.contents = [[],[],[],[],[]]
		self.ind = 0
		self.priorities = [[],[],[],[],[]]
		self.com_acts = {}
	#Using the comedian_List and demographic_List, the this class will create a timetable of slots for each of the 5 work days of the week.
	#The slots are labelled 1-5, and so when creating the timetable, they can be assigned as such:
	#	timetableObj.addSession("Monday", 1, comedian_Obj, demographic_Obj, "main")
	#This line will set the session slot '1' on Monday to a main show with comedian_obj, which is being marketed to demographic_obj.
	#Note here that the comedian and demographic are represented by objects, not strings.
	#The day (1st argument) can be assigned the following values: "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"
	#The slot (2nd argument) can be assigned the following values: 1, 2, 3, 4, 5 in Task 1 and 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 in Tasks 2 and 3.
	#Comedian (3rd argument) and Demographic (4th argument) can be assigned any value, but if the comedian or demographic are not in the original lists,
	#	your solution will be marked incorrectly.
	#The final, 5th argument, is the show type. For Task 1, all shows should be "main". For Tasks 2 and 3, you should assign either "main" or "test" as the show type.
	#In Tasks 2 and 3, all shows will either be a 'main' show or a 'test' show

	#demographic_List is a list of Demographic objects. A Demographic object, 'd' has the following attributes:
	# d.reference  - the reference code of the demographic
	# d.topics - a list of strings, describing the topics that the demographic like to see in their comedy shows e.g. ["Politics", "Family"]

	#comedian_List is a list of Comedian objects. A Comedian object, 'c', has the following attributes:
	# c.name - the name of the Comedian
	# c.themes - a list of strings, describing the themes that the comedian uses in their comedy shows e.g. ["Politics", "Family"]

	#For Task 1:
	#Keep in mind that a comedian can only have their show marketed to a demographic
	#	if the comedian's themes contain every topic the demographic likes to see in their comedy shows.
	#Furthermore, a comedian can only perform one main show a day, and a maximum of two main shows over the course of the week.
	#There will always be 25 demographics, one for each slot in the week, but the number of comedians will vary.
	#In some problems, demographics will have 2 topics and in others 3 topics.
	#A comedian will have between 3-8 different themes.

	#For Tasks 2 and 3:
	#A comedian can only have their test show marketed to a demographic if the comedian's themes contain at least one topic
	#	that the demographic likes to see in their comedy shows.
	#Comedians can only manage 4 hours of stage time a week, where main shows are 2 hours and test shows are 1 hour.
	#A Comedian cannot be on stage for more than 2 hours a day.

	#You should not use any other methods and/or properties from the classes, these five calls are the only methods you should need.
	#Furthermore, you should not import anything else beyond what has been imported above.
	#To reiterate, the five calls are timetableObj.addSession, d.reference, d.topics, c.name, c.themes

	#This method should return a timetable object with a schedule that is legal according to all constraints of Task 1.
	#Sort the demographics in order of hardest to constrain



	#In task 1, I ordered comedians and demograhics in order of how many valid assignments each has, from lowest to highest. 
	# I then performed recursive backtracking, which was minimised by the hueristic applied.

	def sortCount(self,demos): #A merge sort that sorts a list by an attached value
		if len(demos) < 2:
			return demos
		mid = len(demos) // 2
		s1 = demos[:mid]
		s2 = demos[mid:]
		s1 = self.sortCount(s1)
		s2 = self.sortCount(s2)
		ind1 = 0
		ind2 = 0
		s3 = []
		while ind1 < len(s1) and ind2 < len(s2):
			if s1[ind1][1] < s2[ind2][1]:
				s3.append(s1[ind1])
				ind1 += 1
			else:
				s3.append(s2[ind2])
				ind2 += 1
		while ind1 < len(s1):
			s3.append(s1[ind1])
			ind1 += 1
		while ind2 < len(s2):
			s3.append(s2[ind2])
			ind2 += 1
		return s3

	
	def validAct(self,comedian,demo): #Checks if a comedian has all the themes a demo wants
		topics = demo.topics
		for t in topics:
			if t not in comedian.themes:
				return False
		return True

	def prepareDemos(self): #Pairs each demographic with how many acts are valid for it
		validDemoActs = []
		for i in range(len(self.demographic_List)): #Finds how many acts each demographic is appealed to by
			temp = [self.demographic_List[i],0]
			for act in self.comedian_List:
				if self.validAct(act,self.demographic_List[i]):
					temp[1] += 1
			validDemoActs.append(temp)
		return validDemoActs

	def prepareComedians(self): #Pairs each comedian with how many demos are valid for them
		validComDemos = []
		for i in range(len(self.comedian_List)): #Finds how many demographics each comedian can appeal to
			temp = [self.comedian_List[i],0]
			for demo in self.demographic_List:
				if self.validAct(self.comedian_List[i],demo):
					temp[1] += 1
			validComDemos.append(temp)
		return validComDemos

	def assignValues(self,demos,acts):#Recursively inds a valid set of assignments
		if len(demos) == 1:
			for act in acts: #Checks if final demographic has a valid comedian
				if self.validAct(act,demos[0]):
					self.addToSchedule(demos[0],act)
					return True
		else:
			prev = None #Variable used to skip acts that appear twice in a row
			for act in acts: 
				if act != prev and self.validAct(act,demos[0]):#Deepens recursive call for valid comedians
					temp = acts[:]
					temp.remove(act) #Removes current comedian from list of current comedians
					if self.assignValues(demos[1:],temp):
						self.addToSchedule(demos[0],act) #Adds valid demographic and comedian to schedule if it is a valid assignment
						return True
				prev = act
		return False

	def checkDupe(self,comedian,day):#Ensures a comedian can't appear twice in a day
		for i in day:
			if i[1] == comedian:
				return True
		return False		
		
	def addToSchedule(self,demo,act):
		indx = self.ind #Attempts to distribute values evenly 
		count = 0 

		while len(self.contents[indx]) > 4 or self.checkDupe(act,self.contents[indx]):#Attempts to ensure you can't assign to full or invalid days
			indx += 1
			indx = indx % 5
			count += 1
			if count == 5: #If the only day with space has the act, assigns it then swaps it with an act from a different day
				while len(self.contents[indx]) > 4: #Finds a non full day
					indx += 1
					indx = indx % 5
				self.contents[indx].append([demo,act])
				for j in range(len(self.contents[(indx+1) % 5])-1,0,-1): #Swap act with an act that would be valid in the current day.
					if not self.checkDupe(self.contents[(indx+1) % 5][j][1],self.contents[indx]):
						temp = self.contents[(indx+1) % 5][j]
						self.contents[(indx+1) % 5][j] = self.contents[indx][-1]
						self.contents[indx][-1] = temp
						break
				break
		if(count < 5):
			self.contents[indx].append([demo,act]) #Assigns the current act to an empty valid postion if it hasn't been assigned and swapped already
		indx += 1
		indx = indx % 5
		self.ind = indx


	def createSchedule(self):
		#Do not change this line
		timetableObj = timetable.Timetable(1)

		validDemoActs = self.prepareDemos()
		validDemoActs = self.sortCount(validDemoActs) #Sorts demos by how many acts satisfy them
		sortedDemos = []
		for i in validDemoActs:
			sortedDemos.append(i[0]) #Reverts sorted demos back to just demos
		
		validComDemos = self.prepareComedians()

		validComDemos = self.sortCount(validComDemos) #Sorts comedians by how many demos they satisfy

		double_List = []
		for i in validComDemos: #Doubles up the acts as they can appear twice a week
			double_List.append(i[0])
			double_List.append(i[0])

		self.contents = [[],[],[],[],[]]
		self.assignValues(sortedDemos,double_List) #Assigns values

		days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
		for i in range(5):#Fills in the timetableObj
			for j in range(len(self.contents[i])):
				timetableObj.addSession(days[i], j + 1, self.contents[i][j][1], self.contents[i][j][0], "main")

		return timetableObj

	#Now, for Task 2 we introduce test shows. Each day now has ten sessions, and we want to market one main show and one test show
	#	to each demographic.
	#All slots must be either a main or a test show, and each show requires a comedian and a demographic.
	#A comedian can have their test show marketed to a demographic if the comedian's themes include at least one topic the demographic likes.
	#We are also concerned with stage hours. A comedian can be on stage for a maximum of four hours a week.
	#Main shows are 2 hours long, test shows are 1 hour long.
	#A comedian cannot be on stage for more than 2 hours a day.


	
	#In task 2, I performed a very similiar method as part 1, but with test acts added to the end since they are more likely to have a higher number of compatible demographics

	def prepareTestDemos(self): #Pairs each demo with how many test shows are valid for it
		validDemoActs = []
		for i in range(len(self.demographic_List)): 
			temp = [self.demographic_List[i],0]
			for act in self.comedian_List:
				if self.validTestAct(act,self.demographic_List[i]):
					temp[1] += 1
			validDemoActs.append(temp)
		return validDemoActs

	def validTestAct(self,comedian,demo):
		topics = demo.topics
		for t in topics:
			if t in comedian.themes:
				return True
		return False

	def assignValuesMain(self,demos,acts,testdemos): #Near identical to assignValues, but moves on to assigning test values after all main acts are considered
		prev = None
		if len(demos) == 0:
			return self.assignValuesTest(testdemos,acts)
		else:
			for act in acts:
				if act != prev and self.validAct(act,demos[0]):
					temp = acts[:]
					temp.remove(act) #Remove an act twice as if it is a main act it uses 2 of a comedian's 4 hours
					temp.remove(act)
					if self.assignValuesMain(demos[1:],temp,testdemos):
						self.addToTestSchedule(demos[0],act,2)
						return True
				prev = act
		return False
	
	def assignValuesTest(self,demos,acts): #Near identical to assignValues, but with test acts.
		if len(demos) == 0:
			return True
		else:
			prev = None
			for act in acts:
				if act != prev and self.validTestAct(act,demos[0]):
					temp = acts[:]
					temp.remove(act)
					if self.assignValuesTest(demos[1:],temp):
						self.addToTestSchedule(demos[0],act,1)
						return True
				prev = act
		return False

	def checkOveruse(self,comedian,day,duration): #Ensures a comedian can't have over 2 hours of stage time a day
		counter = 0
		for i in day:
			if i[1] == comedian:
				counter += i[2]
				if counter + duration > 2:
					return True
		return False	

	def addToTestSchedule(self,demo,act,duration): #Near identical to addToSchedule, but with allows more values in contents and calls checkOveruse
		indx = self.ind
		count = 0
		while len(self.contents[indx]) > 9 or self.checkOveruse(act,self.contents[indx],duration):
			indx += 1
			indx = indx % 5
			count += 1
			if count == 5:
				while len(self.contents[indx]) > 9:
					indx += 1
					indx = indx % 5
				self.contents[indx].append([demo,act,duration])
				for j in range(len(self.contents[(indx+1) % 5])):
					if not self.checkOveruse(self.contents[(indx+1) % 5][j][1],self.contents[indx],duration):
						temp = self.contents[(indx+1) % 5][j]
						self.contents[(indx+1) % 5][j] = self.contents[indx][-1]
						self.contents[indx][-1] = temp
						break
				break
		if(count < 5):
			self.contents[indx].append([demo,act,duration])
		indx += 1
		indx = indx % 5
		self.ind = indx



	def createTestShowSchedule(self):
		#Do not change this line
		timetableObj = timetable.Timetable(2)


		validDemoActs = self.prepareDemos() #Pair each demo with valid acts, sort them by them, then seperate them again
		validDemoActs = self.sortCount(validDemoActs)
		sortedDemos = []
		for i in validDemoActs:
			sortedDemos.append(i[0])

		validDemoActs = self.prepareTestDemos() #As above but for test acts
		validDemoActs = self.sortCount(validDemoActs)
		sortedTestDemos = []
		for i in validDemoActs:
			sortedTestDemos.append(i[0])

		validComDemos = self.prepareComedians()
		validComDemos = self.sortCount(validComDemos)
		quad_List = []
		for i in validComDemos: #Add comedian to list for each hour it can be on in the week
			quad_List.append(i[0])
			quad_List.append(i[0])
			quad_List.append(i[0])
			quad_List.append(i[0])
			
		self.contents = [[],[],[],[],[]]
		self.assignValuesMain(sortedDemos,quad_List,sortedTestDemos) #Assigns values
		
		days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
		for i in range(5):
			for j in range(len(self.contents[i])):
				if self.contents[i][j][2] == 2:
					timetableObj.addSession(days[i], j + 1, self.contents[i][j][1], self.contents[i][j][0], "main")
				else:
					timetableObj.addSession(days[i], j + 1, self.contents[i][j][1], self.contents[i][j][0], "test")
		return timetableObj


	#Now, in Task 3 it costs £500 to hire a comedian for a single main show.
	#If we hire a comedian for a second show, it only costs £300. (meaning 2 shows cost £800 compared to £1000)
	#If those two shows are run on consecutive days, the second show only costs £100. (meaning 2 shows cost £600 compared to £1000)

	#It costs £250 to hire a comedian for a test show, and then £50 less for each extra test show (£200, £150 and £100)
	#If a test shows occur on the same day as anything else a comedian is in, then its cost is halved.

	#Using this method, return a timetable object that produces a schedule that is close, or equal, to the optimal solution.
	#You are not expected to always find the optimal solution, but you should be as close as possible.
	#You should consider the lecture material, particular the discussions on heuristics, and how you might develop a heuristic to help you here.


	#My task 3 works by first finding a valid assignment, then improving it.
	#It uses a stochastic approach with a hueristic that orders comedians in order of how likely they are to be cheaper if they steal acts from others
	#It sometimes randomly redistributes acts and restarts itself to try to find the best possible solution
	#Afterwards, it assigns the acts in the optimal day slots.

	def calculatePriority(self,act): #A simple function to assign modification priority to 
		if len(self.com_acts[act][0]) == 2:
			val = 4
		else:
			val = len(self.com_acts[act][1])
		return (4 - val)
 
	def optimise(self,com,priority,acttype):
		replacements = self.findReplacements(priority,com,acttype) #Make it so if a comedian steals acts, it has all main or all test acts
		if None in replacements: #If one of a comedian's acts can't be replaced, then or 4 test isn't attainable
			return False
		if acttype == 1: #If finding test acts, tries to steal from comedians with the least test acts first
			candidates = self.stealActs(com,acttype,int(4/(2-acttype) - len(self.com_acts[com][acttype])),range(3,0,-1))
		else: #If stealing main acts, tries to steal from comedians with the most test acts first
			candidates = self.stealActs(com,acttype,int(4/(2-acttype) - len(self.com_acts[com][acttype])),range(2,5))
		if len(candidates) == 0: #If no swaps can occur, return false
			return False
		self.replaceX(com,replacements,acttype) #Swaps out the replacements and swaps in the stolen acts
		self.performTheft(com,candidates,acttype)
		return True

	def findReplacements(self,priority,com,acttype):
		invalids = [] #Makes sure each comedian can only get act to distribute as evenly as possible
		replacements = []
		for x in self.com_acts[com][1 - acttype]:
			temp = self.findReplacement(priority,x[1],x[0],1 - acttype,invalids) #If a valid comedian that could take over this act is found, adds it to the list
			invalids.append(temp)
			replacements.append(temp)
			if temp == None:
				return replacements
			for i in range(priority + 1,4): #Shuffles each priority so that it is less likely the same combination gets picked between runs
				random.shuffle(self.priorities[i])
		return replacements

	def findReplacement(self,priority,com,demo,acttype,invalids):
		for i in range(4,priority - 1,-1): #Attempt to distribute to acts who have the least. Higher priority acts would've stolen the act being replaced beforehand
			for j in self.priorities[i]: 
				if j != com and (j not in invalids) and self.validAct(j,demo) and (2 - acttype) + 2 * len(self.com_acts[j][0]) + len(self.com_acts[j][1]) <= 4:
					return j #Adds an act to possible replacements if it has a spare valid act
		return None

	def stealActs(self,com,acttype,max,order):
		candidates = []
		thresholds = [0,0,0,0]
		for i in order:
			thresholds[i-1] = len(candidates)#Keeps track of how many acts a comedian can steal before stealing from a higher priority
			for j in self.priorities[i]:
				for k in self.com_acts[j][acttype]:
					if k[1] != com and self.validAct(com,k[0]): #If an act could be stolen from a comedian, add it to list to be stolen
						if i == 3 and max - len(candidates) > 1:#If a comedian wants to steal from a comedian with 2 test acts, it will be a loss unless both or stolen or it is the only one that can be stolen
							steal_both = True;
							for n in self.com_acts[j][acttype]: #Sees if both can be stolen
								if not self.validAct(com,n[0]):
									steal_both = False
							if steal_both:
								for n in self.com_acts[j][acttype]:
									candidates.append(n)
						else:
							candidates.append(k)
						if len(candidates) >= max: #Don't keep searching if you have the most possible acts
							return candidates
						break
		if acttype == 0: #If a comedian fails to obtain 2 main acts there will be no price reduction
			return []
		size = 5
		while size != len(candidates):
			size = len(candidates)
			candidates = candidates[:thresholds[max - len(candidates) - 1]]
		#A comedian can not steal acts that would bring their priority to the level of someone they stole from without losing money
		#Therefore thresholds stores where to cut off invalid comedians to steal from. It is irrelevant for main act
		return candidates

	def replaceX(self,com,replacements,acttype): #Reajusts priorities and swaps around acts determined earlier
		self.priorities[self.calculatePriority(com)].remove(com)
		for i in range (len(replacements)): 
			self.priorities[self.calculatePriority(replacements[i])].remove(replacements[i])
			self.com_acts[com][1-acttype][i][1] = replacements[i]
			self.com_acts[replacements[i]][1 - acttype].append(self.com_acts[com][1-acttype][i])
			self.priorities[self.calculatePriority(replacements[i])].append(replacements[i])
		for i in replacements:
			self.com_acts[com][1 - acttype].remove(self.com_acts[com][1-acttype][0])
		self.priorities[self.calculatePriority(com)].append(com)
		
	
	def performTheft(self,com,candidates,acttype): #Reajusts priorities and swaps around acts determined earlier
		for i in candidates:
			self.priorities[self.calculatePriority(i[1])].remove(i[1])
			self.priorities[self.calculatePriority(com)].remove(com)
			temp = i[:]
			temp[1] = com
			self.com_acts[com][acttype].append(temp)
			self.com_acts[i[1]][acttype].remove(i)
			self.priorities[self.calculatePriority(i[1])].append(i[1])
			self.priorities[self.calculatePriority(com)].append(com)

	def copyComActs(self,acts): #Copies contents of large dictionary via value instead of reference
		temp = {act: [] for act in self.comedian_List}
		for i in self.comedian_List:
			temp2 = []
			for j in acts[i]:
				temp3 = []
				for k in j:
					temp3.append(k.copy())
				temp2.append(temp3)
			temp[i] = temp2
		return temp

	def copyPris(self,pris):
		temp = []
		for i in pris:
			temp.append(i.copy())
		return temp

	def createMinCostSchedule(self):
		#Do not change this line
		timetableObj = timetable.Timetable(2)

		self.createTestShowSchedule() #Uses the previous task to calculate a valid schedule

		self.com_acts = {act: [[],[]] for act in self.comedian_List}
	
		for i in self.contents:
			for j in i:
				if j[2] == 2:
					self.com_acts[j[1]][0].append(j[:])
				else:
					self.com_acts[j[1]][1].append(j)

		self.priorities = [[],[],[],[],[]] #Highest to lowest pri 4 test or 2 main, 3 tests, 2 test, 1 test, unused

		for act in self.comedian_List:
			self.priorities[self.calculatePriority(act)].append(act) #Stores current priority of each comedian
		
		no_change = False
		original = self.copyComActs(self.com_acts)
		originalpri = self.copyPris(self.priorities)
		best = self.copyComActs(self.com_acts)
		bestpri = self.copyPris(self.priorities)
		cost = 0
		cost += len(self.priorities[1]) * 375 + len(self.priorities[2]) * 225 + len(self.priorities[3]) * 150 #Calculates cost of current assignment if arranged optimally
		for act in self.priorities[0]: 
			if len(self.com_acts[act][0]) > 0:
				cost -= 400
			else:
				cost += 350
		restart = 0
		restarts = 0
		while random.randint(0,int(restart)) == 0: #randomly restarts the assignment in case a better one can be found. Always restarts first assignment
			rand = 2
			restarts += 1
			while not no_change:
				no_change = True
				for i in range(1,5):
					for act in self.priorities[0]: #Will randomly redistribute values
						if random.randint(0,int(rand)) == 0:
							replacements = self.findReplacements(1,act,0)
							if None in replacements: #Does not care if not all acts can be redistributed, just does the ones that can be
								replacements.remove(None)
							self.replaceX(act,replacements,0)
							replacements = self.findReplacements(1,act,1)
							if None in replacements:
								replacements.remove(None)
							self.replaceX(act,replacements,1)
				for i in range(1,5):
					for act in self.priorities[i]: #Allows a comedian to steal test acts, starting with comedians that will more easily get four of them
						if self.optimise(act,i,1):
							no_change = False
				for i in range(4,0,-1):
					for act in self.priorities[i]: #Allows comedians to steal main acts, starting with ones that will have to give away the least test acts
						if self.optimise(act,i,0):
							no_change = False
				rand += rand / 2#Decreases likelihood of redistributed acts

			for act in self.priorities[3]:#Allow any comedians with 1 test act to steal an act from a comedian with 3.
				candidates = self.stealActs(act,1,1,range(1,2))
				self.performTheft(act,candidates,1)

			cost2 = 0 #Calculates cost of latest assignment
			cost2 += len(self.priorities[1]) * 375 + len(self.priorities[2]) * 225 + len(self.priorities[3]) * 150
			for act in self.priorities[0]:
				if len(self.com_acts[act][0]) > 0:
					cost2 -= 400
				else:
					cost2 += 350
			if(cost > cost2): #If a cheaper assignment is found, replaces the old one
				cost = cost2
				best = self.copyComActs(self.com_acts)
				bestpri = self.copyPris(self.priorities)
				restart = 0 #Resets likelihood of restart
			else:
				restart += 0.02 #Decreases likelihood of restart
			if random.randint(0,1) == 0:
				self.com_acts = self.copyComActs(original) #Reverts to original to find possible superior
				self.priorities = self.copyPris(originalpri)

		self.com_acts = self.copyComActs(best) #Reloads the best assignment
		self.priorities = self.copyPris(bestpri)
		self.contents = [[],[],[],[],[]] #Prepares the contents 
		groupings = [[],[],[]] #Pairs of mains should be assigned first, then pairs of tests, then others

		#Groups acts appropriately
		for act in self.priorities[0]: 
			acttype = int(len(self.com_acts[act][1]) / 4)
			for i in self.com_acts[act][acttype]: #Appends pairs of main acts and quadruple test acts
				groupings[acttype].append(i)
		for i in range (1,3): #Assigns the pairs of test acts
			for act in self.priorities[i]:
				groupings[1].append(self.com_acts[act][1][0])
				groupings[1].append(self.com_acts[act][1][1])
				self.com_acts[act][1] = self.com_acts[act][1][2:] 
		for i in range (1,4,2):#Assigns the remaining test acts
			for act in self.priorities[i]:
				groupings[2].append(self.com_acts[act][1][0])
		for i in range(2,5): #Assigns the remaining main acts
			for act in self.priorities[i]:
				if len(self.com_acts[act][0]) == 1:
					groupings[2].append(self.com_acts[act][0][0])

		#This block of code assigns acts to slots in a way so that the lowest cost positions will be achieved
		count = 0
		for i in range (0,len(groupings[0]),2): #Tiles pairs of acts across subsequent days, doubling up on thursdays to ensure optimal layout
			temp = count
			if temp == 4:
				temp = 3
			self.contents[temp].append(groupings[0][i])
			self.contents[temp + 1].append(groupings[0][i+1])
			count = (count + 2) % 6
		count = 0
		for i in range (0,len(groupings[1]),2): #Adds pairs of tests to the same day. There will always be spaced out
			while len(self.contents[count]) > 8 or self.checkOveruse(groupings[1][i][1],self.contents[count],2):
				count = (count + 1) % 5
			self.contents[count].append(groupings[1][i])
			self.contents[count].append(groupings[1][i+1])
			count = (count + 1) % 5
		self.ind = 0
		for i in groupings[2]: #Adds the rest of the acts
			self.addToTestSchedule(i[0],i[1],i[2])

		#Categories in order of priority: 1 main, 3 test, 1 test
		days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
		for i in range(5):
			for j in range(len(self.contents[i])):
				if self.contents[i][j][2] == 2:
					timetableObj.addSession(days[i], j + 1, self.contents[i][j][1], self.contents[i][j][0], "main")
				else:
					timetableObj.addSession(days[i], j + 1, self.contents[i][j][1], self.contents[i][j][0], "test")
		return timetableObj


		


	#This simplistic approach merely assigns each demographic and comedian to a random slot, iterating through the timetable.
	def randomMainSchedule(self,timetableObj):

		sessionNumber = 1
		days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
		dayNumber = 0
		for demographic in self.demographic_List:
			comedian = self.comedian_List[random.randrange(0, len(self.comedian_List))]

			timetableObj.addSession(days[dayNumber], sessionNumber, comedian, demographic, "main")

			sessionNumber = sessionNumber + 1

			if sessionNumber == 6:
				sessionNumber = 1
				dayNumber = dayNumber + 1

	#This simplistic approach merely assigns each demographic to a random main and test show, with a random comedian, iterating through the timetable.
	def randomMainAndTestSchedule(self,timetableObj):

		sessionNumber = 1
		days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"]
		dayNumber = 0
		for demographic in self.demographic_List:
			comedian = self.comedian_List[random.randrange(0, len(self.comedian_List))]

			timetableObj.addSession(days[dayNumber], sessionNumber, comedian, demographic, "main")

			sessionNumber = sessionNumber + 1

			if sessionNumber == 11:
				sessionNumber = 1
				dayNumber = dayNumber + 1

		for demographic in self.demographic_List:
			comedian = self.comedian_List[random.randrange(0, len(self.comedian_List))]

			timetableObj.addSession(days[dayNumber], sessionNumber, comedian, demographic, "test")

			sessionNumber = sessionNumber + 1

			if sessionNumber == 11:
				sessionNumber = 1
				dayNumber = dayNumber + 1
