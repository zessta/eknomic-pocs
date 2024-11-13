import React, { useEffect, useState } from 'react';
import { FlatList, Text, View, StyleSheet, Image } from 'react-native';
import { database } from '../../components/firebaseConfig'; // Correct Firebase import
import { ref, onValue } from '@react-native-firebase/database'; // Firebase database functions
import { Link } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';
import { useLocalSearchParams } from 'expo-router';

export interface User {
    id: string;
    name: string;
    avatar?: string;
}

const UserListScreen: React.FC = ({ navigation }: any) => {
    const [users, setUsers] = useState<User[]>([]);
    const { senderUserId, senderUserName } = useLocalSearchParams();

    useEffect(() => {
        // Set up the reference to the Firebase Realtime Database
        const usersRef = ref(database, 'users'); // Reference to 'users' node

        const unsubscribe = onValue(usersRef, (snapshot) => {
            const data = snapshot.val(); // Get the data from the snapshot
            const usersList: User[] = data ? Object.keys(data).map(key => ({ id: key, ...data[key] })) : [];
            setUsers(usersList); // Update the state with the fetched users
        });

        // Cleanup the listener when the component unmounts
        return () => unsubscribe();
    }, []);

    // Find sender's name from the users list
    // const senderUserName = users.length ? users.find((user) => user.id === senderUserId)!?.name : '';

    return (
        <View style={styles.container}>
            <View style={styles.header}>
                <Text style={styles.headerText}>Buyers</Text>
                <MaterialIcons name="group" size={24} color="#fff" />
            </View>
            <FlatList
                data={users}
                keyExtractor={item => item.id}
                renderItem={({ item }) => {
                    // Exclude the sender user from the list
                    if (item.id !== senderUserId) {
                        return (
                            <Link
                                style={styles.userCard}
                                href={{
                                    pathname: '/chat',
                                    params: { receiverUserId: item.id, receiverUserName: item.name, senderUserName, senderUserId }
                                }}>
                                <Image source={{ uri: item.avatar || 'https://ui-avatars.com/api/?name=User' }} style={styles.avatar} />
                                <View style={styles.userInfo}>
                                    <Text style={styles.userName}>{item.name}</Text>
                                    <Text style={styles.userStatus}>Active</Text>
                                </View>
                                <MaterialIcons name="chevron-right" size={24} color="#B0BEC5" />
                            </Link>
                        );
                    }
                    return null;
                }}
                contentContainerStyle={styles.listContent}
            />
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#E3F2FD',
        paddingTop: 40,
    },
    header: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        backgroundColor: '#1976D2',
        padding: 15,
        borderBottomLeftRadius: 20,
        borderBottomRightRadius: 20,
        elevation: 5,
    },
    headerText: {
        fontSize: 24,
        color: '#fff',
        fontWeight: 'bold',
    },
    userCard: {
        flexDirection: 'row',
        alignItems: 'center',
        backgroundColor: '#fff',
        padding: 15,
        marginVertical: 8,
        marginHorizontal: 16,
        borderRadius: 10,
        elevation: 2,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 1 },
        shadowOpacity: 0.3,
        shadowRadius: 2,
    },
    avatar: {
        width: 50,
        height: 50,
        borderRadius: 25,
        marginRight: 15,
    },
    userInfo: {
        flex: 1,
    },
    userName: {
        fontSize: 18,
        fontWeight: '600',
        color: '#333',
    },
    userStatus: {
        fontSize: 14,
        color: '#B0BEC5',
    },
    listContent: {
        paddingBottom: 20,
    },
});

export default UserListScreen;
