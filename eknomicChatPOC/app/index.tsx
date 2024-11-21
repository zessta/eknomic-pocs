import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, Button, StyleSheet, Alert, ActivityIndicator } from 'react-native';
import 'react-native-get-random-values'

// Make sure to import @react-native-firebase/app to initialize Firebase
import '@react-native-firebase/app'; // Firebase initialization
import { ref, set, onValue } from '@react-native-firebase/database'; 
import { useNavigation, useRouter } from 'expo-router';
// import database from '@react-native-firebase/database'; // Firebase Database module
import { database } from '../components/firebaseConfig'; // Adjust the path accordingly
import { startBackgroundSync } from '@/components/BackgroundFetch';

interface User {
  id: string;
  name: string;
  avatar?: string;
}

const UserScreen: React.FC = () => {
  const [name, setName] = useState<string>('');
  const [avatar, setAvatar] = useState<string>('');
  const [users, setUsers] = useState<Record<string, User>>({});
  const [loading, setLoading] = useState<boolean>(false); // Loading state
  const router = useRouter();
  const navigation = useNavigation();

  // Initialize Firebase if it hasn't been initialized already
  useEffect(() => {
    navigation.setOptions({ title: 'Eknomic' });
  }, [navigation]);
  
  useEffect(() => {
    // Start the background sync task
    startBackgroundSync();
  }, []);
  // Load users from Firebase Realtime Database
  useEffect(() => {
    setLoading(true); // Start loading when fetching users
    const usersRef = ref(database, 'users');
    const unsubscribe = onValue(usersRef, snapshot => {
      const data = snapshot.val();
      setLoading(false); // Stop loading when data is fetched
      if (data) {
        setUsers(data); // Store users in state
      } else {
        console.log('No users found in the database.');
      }
    });

    return () => unsubscribe(); // Cleanup listener on unmount
  }, []);
  console.log('users', users)
  const handleCreateUser = async () => {
    if (!name) {
      Alert.alert('Please enter your name.');
      return;
    }

    // Check if user already exists
    const existingUser = Object.values(users).find(user => user.name === name);
    console.log('existingUser', existingUser)
    if (existingUser) {
      console.log('coming insinde existing user')
      // If user exists, navigate to chat screen with existing user ID
      router.push(`/(tabs)/chatTest?senderUserId=${existingUser.id}&senderUserName=${existingUser.name}`);
    } else {
      setLoading(true); // Start loading while creating a new user
      try {
        // Generate a new user ID using Firebase's push method
        const newUserRef = ref(database, 'users').push();
        const newUserId = newUserRef.key; // Automatically generated key
        console.log('Creating new user with ID:', newUserId);

        await set(newUserRef, {
          name,
          avatar,
          id: newUserId, // Ensure the user object has an `id` key
        });

        // Navigate to chat screen with the new user's ID
        router.push(`/(tabs)/chatTest?senderUserId=${newUserId}`);
      } catch (error) {
        setLoading(false); // Stop loading on error
        Alert.alert('Error', 'There was an issue creating the user. Please try again.');
        console.error('Error creating user:', error);
      }
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

      <Button title="Create User" onPress={handleCreateUser} disabled={loading} />

      {loading && <ActivityIndicator size="large" color="#0000ff" style={styles.loading} />}
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
  loading: {
    marginTop: 20,
  },
});

export default UserScreen;
