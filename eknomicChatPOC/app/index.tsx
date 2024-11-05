import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, Button, StyleSheet, Alert } from 'react-native';
import { ref, set, onValue } from '@react-native-firebase/database';
import { database } from '../components/firebaseConfig';  // Import Firebase database correctly
import { useNavigation, useRouter } from 'expo-router';

interface User {
  id: string;
  name: string;
  avatar?: string;
}

const UserScreen: React.FC = () => {
  const [name, setName] = useState<string>('');
  const [avatar, setAvatar] = useState<string>('');
  const [users, setUsers] = useState<Record<string, User>>({});
  const router = useRouter();
  const navigation = useNavigation();

  useEffect(() => {
    navigation.setOptions({ title: 'Eknomic' });
  }, [navigation]);

  // Load users from Firebase database
  useEffect(() => {
    const usersRef = ref(database, 'users');
    const unsubscribe = onValue(usersRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        // Log the data to ensure it's being retrieved correctly
        console.log('Fetched users:', data);
        setUsers(data); // Store users in state
      } else {
        console.log('No users found in the database.');
      }
    });

    return () => unsubscribe(); // Cleanup listener on unmount
  }, []);
  console.log('suers', users)

  useEffect(() => {
    console.log('Updated users state:', users); // Log users state whenever it changes
  }, [users]); // This will trigger whenever users state updates

  const handleCreateUser = async () => {
    if (!name) {
      Alert.alert('Please enter your name.');
      return;
    }

    // Check if user already exists
    const existingUser = Object.values(users).find(user => user.name === name);
    if (existingUser) {
      // If user exists, navigate to chat screen with existing user ID
      router.push(`/(tabs)/chatTest?senderUserId=${existingUser.id}`);
    } else {
      // Create a new user
      const userId = new Date().getTime().toString(); // Simple user ID based on timestamp
      const userRef = ref(database, `users/${userId}`);
        console.log('userId', userId)
      await set(userRef, {
        name,
        avatar,
        id: userId, // Ensure the user object has an `id` key
      });

      // Navigate to chat screen with the new user's ID
      router.push(`/(tabs)/chatTest?senderUserId=${userId}`);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Login</Text>
      <TextInput
        style={styles.input}
        placeholder="Name"
        value={name}
        onChangeText={setName}
      />
      <TextInput
        style={styles.input}
        placeholder="Avatar URL (optional)"
        value={avatar}
        onChangeText={setAvatar}
      />
      <Button title="Create User" onPress={handleCreateUser} />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    padding: 16,
    backgroundColor: '#E3F2FD',
  },
  title: {
    fontSize: 24,
    marginBottom: 20,
    textAlign: 'center',
  },
  input: {
    height: 40,
    borderColor: 'gray',
    borderWidth: 1,
    marginBottom: 12,
    paddingHorizontal: 10,
    borderRadius: 5,
  },
});

export default UserScreen;
