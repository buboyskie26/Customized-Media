# Customized-Media
Core functionalities of twitter-facebook

### Simple Social Site
For the sake of having fun and for being curious on the web. I decided to explore something and found out that social media is the great way to spend my time so I dived into it and in order for me to enchance my .Net Core Web Api, I created this simple Social network which has a core functionality of social media such as.

- Creating a feed
- Creating an activity
- Requesting to join to the activity
- Viewing someone feed (which depends if you followed someone)
- Like/Heart a post
- Follow User
- Unfollow User
- Sharing someone post in public/private.
- Having notification if someone shared/list/heart/request your post/activity.
- Tagging someone to the existing/creating post
- Searching users that shows their PUBLIC activity feed.
- Commenting someone post
- Creating a group
- Adding other user to the group
- Removing existing user in the group
- Blocking someone which you hated the most
- One to one Message with sending apicture or file
- Group Message with sending apicture or file

### Rules In Every Controller
1. User who created Activity only could see the request users and registered user in that activity.
 
2. User could SET/CREATE an Activity that other user can joined that activity

3. User that had joined that specific activity has a capability to comment and follow that user INSIDE OF THAT ACTIVITY
4. User whom created the activity could only accept the user to be in/belong on that activity.
5. User whom created the activity could only reject the user on that activity.

6. User post activity feed view depending on their follow

* If the user doesnt followed anyone, or that user doesnt have a include post from other user his own post ( wether private or public ) and shared posts would be reflected on his activity feed view.

* if the user doesnt followed anyone but other users included him to see their shared post of that user, the activity feed that will reflects is the users (public or private post) and shared post and the shared post of the user who had included him/her to see that specific post.

* If the user who followed someone but that user doesnt have a include post from other user his activity feed view would reflected his own post ( wether private or public ) and the posts and shared posts of other user that he/she follows (only public post).

* If the user who followed someone and that user does included him to the post from other users, his activity feed view will reflects the posts/activity and shared post of the user that she/he follows and the exclusive posts from the other user who had included him to see his shared posts.

 CASE 1 

If the sharedPost user who set his owned post to isOnlyMe, that posts would reflect to his activity feed, but the other user who had followerd him could not see his sharedPost to that specific activity. 

 CASE 2

If the sharedPost user who set their post in the activity into only me, all of the other users who followed that guy wouldnt see his shared post to that specific activity post.

### ActivityAttendee Controler rules

1. User who are belong to the activity/room he/she wanted to belong could request to join.
2. User who requested to belong in the activity has a capability to delete his requests.

### Comment Controller Rules.
1. User who are belong to the activity could comment to the user who ONLY A MEMBER on that activity.

## Reacting Comments Controller Rules

User who are only accepted in the activity Id must perform reacting other comments user.
User only once could react the specific comment Id.
User who are accepted in the activity Id could perform react to the comments ONLY WHEN that comment is belong to that specific activity Id/Room

## UserFollowing Controller

User could follow other user, and could only see the post of the user that he had followed.

User could post a acitivity.
That activity has a view setting wether the user who created the activity could modify the view setting that if he wants to see his post only by the user who followed him, or all public (all user that are not belong to his followers). 

Public -> non followed user could see the user created post that the post has a public only.

Only me-> User Who created the activity could only see his post.

Friends-> All user activity post could see by his friends only. 

Friends of Friends

Once user post an activity, only his followers could see.

Once user goes to the profile page of the user, If the user who are not followers of the user he had visited => NO POST, ELSE show that user post IF he is one of his followers.

For Sharing A post requirements

If the user who didnt follow other user that he wanted to shared his post is ILLEGAL.
Only once must the user could shared posts from his following users.
User could search other user on homepage.

User could block other users, that user would not be included to the user experience


TagUser Rules

User could tag other users
All user who had been tag by that user would be reflected to their posts and all of their followers would see that tagged post.

User could see their individual tagged posts.

Memories Feature

All activities posts that sets in specific month or year, and if that user activity posts reached that date, it will show.

### Notification rules

If the unknown users request to join in the Activity, the activity creator should receive a notification that tells user from outside have a request to join to your created activity.

// Followers
// Comment
// CommentReaction
// SharingActivity
 - Once the other user who have followed you shared your activity post, you`ll get a notification from that user.


Message Controller Rules

- One to One Message.

- User could message anyone that were registered in the system.
- User could view the latest message in the conversation, either your message or by whom.
- Group Chat Message

User could create a chat room with his followed user.
- Each user could see in the messages box the group chat and could view the latest message in - their respective chat room messages..
- User who created a post couldo nly remove other user which belongs to the created group.
- Other user could add other user to join the group.
- User could leave the group.
- User could remove his own messages.
