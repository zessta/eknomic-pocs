// UserScreen.tsx

import React, { useState } from 'react';
import { View, Text, TextInput, Button, StyleSheet, Alert } from 'react-native';
import { ref, set } from 'firebase/database';
import { database } from './firebaseConfig';
import { useNavigation } from '@react-navigation/native';

const UserScreen = () => {
    const [name, setName] = useState('');
    const [avatar, setAvatar] = useState('');
    const navigation = useNavigation();

    const handleCreateUser = async () => {
        if (!name) {
            Alert.alert('Please enter your name.');
            return;
        }

        const userId = new Date().getTime(); // Simple user ID based on timestamp
        const userRef = ref(database, `users/${userId}`);

        await set(userRef, {
            name,
            avatar,
        });

        navigation.navigate('UserList', { senderUserId: userId });
    };

    return (
        <View style={styles.container}>
            <Text style={styles.title}>Create Your Profile</Text>
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
