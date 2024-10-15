import React, { useEffect, useState } from 'react';
import { FlatList, Text, View, StyleSheet, TouchableOpacity, Image } from 'react-native';
import { ref, onValue } from 'firebase/database';
import { database } from '../../components/firebaseConfig';
import { Link } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';

interface User {
    id: string; // Changed to string to match Firebase keys
    name: string;
    avatar?: string; // Optional avatar URL
}

const UserListScreen = ({ navigation }: any) => {
  const senderUserId = 1;
    const [users, setUsers] = useState<User[]>([]);

    useEffect(() => {
        const usersRef = ref(database, 'users');
        onValue(usersRef, (snapshot) => {
            const data = snapshot.val();
            const usersList: User[] = data ? Object.keys(data).map(key => ({ id: key, ...data[key] })) : [];
            setUsers(usersList);
        });
    }, []);
    console.log('users', users)
    const senderUserName = users.length ? users.find((user) => user.id as unknown as number === senderUserId)?.name : '';
    console.log('senderUserNamefilter', senderUserName)
    return (
        <View style={styles.container}>
            <View style={styles.header}>
                <Text style={styles.headerText}>User List</Text>
                <MaterialIcons name="group" size={24} color="#fff" />
            </View>
            <FlatList
                data={users}
                keyExtractor={item => item.id}
                renderItem={({ item }) => {
                  if(item.id as unknown as number !== senderUserId){
                  return (
                    <Link
                    style={styles.userCard}
                        href={{
                            pathname: '/chat',
                            params: { receiverUserId: item.id, receiverUserName: item.name, senderUserName: senderUserName}
                        }}>
                            <Image source={{ uri: item.avatar || 'https://ui-avatars.com/api/?name=User' }} style={styles.avatar} />
                            <View style={styles.userInfo}>
                                <Text style={styles.userName}>{item.name}</Text>
                                <Text style={styles.userStatus}>Active</Text>
                            </View>
                            <MaterialIcons name="chevron-right" size={24} color="#B0BEC5" />
                    </Link>
                  )  
                  } return null
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
