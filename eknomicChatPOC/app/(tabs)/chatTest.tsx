import React, { useEffect, useState } from 'react';
import { FlatList, Text, TouchableOpacity, View } from 'react-native';
import { ref, onValue } from 'firebase/database';
import { database } from '../../components/firebaseConfig';
import { Link } from 'expo-router';
interface User {
    id: number;
    name: string;
  }
const UserListScreen = ({ navigation }: any) => {
    const [users, setUsers] = useState<User[]>([]);
  
    useEffect(() => {
      const usersRef = ref(database, 'users');
      onValue(usersRef, (snapshot) => {
        const data = snapshot.val();
        console.log('testing data', data)
        const usersList: User[] = data ? Object.keys(data).map(key => ({ id: key, ...data[key] })) : [];
        setUsers(usersList);
      });
  
    //   return () => usersRef.off();
    }, []);
    console.log('users', users)
  
    return (
        <View style={{marginTop: '30%'}}>
      <FlatList
        data={users}
        keyExtractor={item => `${item.id}`}
        renderItem={({ item }) => (
            <Link href={{
                pathname: '/chat',
                params: { userId: item.id }
              }}>
            <Text style={{ padding: 20, marginTop: 10 }}>{item.name}</Text>
          </Link>
        )}
      />
      </View>
    );
  };
  export default UserListScreen;
