import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, Button, StyleSheet, Alert } from 'react-native';
import { ref, set, onValue } from 'firebase/database';
import { database } from '../components/firebaseConfig';
import { useRouter } from 'expo-router';

interface User {
    id: number;
    name: string;
    avatar?: string;
}

const UserScreen: React.FC = () => {
    const [name, setName] = useState<string>('');
    const [avatar, setAvatar] = useState<string>('');
    const [users, setUsers] = useState<Record<string, User>>({});
    const router = useRouter();

    useEffect(() => {
        const usersRef = ref(database, 'users');
        onValue(usersRef, (snapshot) => {
            const data = snapshot.val() || {};
            setUsers(data); // Store the users in state
        });
    }, []);

    const handleCreateUser = async () => {
        if (!name) {
            Alert.alert('Please enter your name.');
            return;
        }

        // Check if the user already exists
        const existingUser = Object.values(users).find(user => user.name === name);

        if (existingUser) {
            // User exists, navigate to chat screen with existing user ID
            router.push(`/(tabs)/chatTest?senderUserId=${existingUser.id}`);
        } else {
            // Create a new user
            const userId = new Date().getTime(); // Simple user ID based on timestamp
            const userRef = ref(database, `users/${userId}`);

            await set(userRef, {
                name,
                avatar,
                userId
            });

            router.push(`/(tabs)/chatTest?senderUserId=${userId}`);
        }
    };

    return (
        <View style={styles.container}>
            <Text style={styles.title}>Eknomic</Text>
            <TextInput
                style={styles.input}
                placeholder="Name"
                value={name}
                onChangeText={setName}
            />
            <Button title="Login" onPress={handleCreateUser} />
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
